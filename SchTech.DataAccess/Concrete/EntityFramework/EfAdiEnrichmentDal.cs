using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfAdiEnrichmentDal : EfEntityRepositoryBase<Adi_Data, ADI_EnrichmentContext>, IAdiDataDal
    {
        public static bool ExpiryProcessing { get; private set; }

        public static bool IsWorkflowProcessing { get; set; }

        private bool IsOrphanCleanupRunning { get; set; }

        private ADI_EnrichmentContext CurrentContext { get; set; }
        
        public bool CleanAdiDataWithNoMapping(bool timerElapsed)
        {
            try
            {
                if (!IsWorkflowProcessing && !IsOrphanCleanupRunning)
                    CheckAndClearOrphanedData();

                if (!ExpiryProcessing && timerElapsed)
                    Task.Run((Action)ClearExpiredAssets);
                return true;
            }
            catch (SqlException sqlEx)
            {
                EfStaticMethods.Log.Error($"SQL Exception during database connection: {sqlEx.Message}");
                if (sqlEx.InnerException != null)
                    EfStaticMethods.Log.Error($"Inner Exception: {sqlEx.InnerException.Message}");

                return false;
            }
            catch (Exception cawndEx)
            {
                EfStaticMethods.Log.Error($"General Exception during database connection: {cawndEx.Message}");
                if (cawndEx.InnerException != null)
                    EfStaticMethods.Log.Error($"Inner Exception: {cawndEx.InnerException.Message}");

                return false;
            }
        }

        private void ClearExpiredAssets()
        {
            ExpiryProcessing = true;

            using (CurrentContext = new ADI_EnrichmentContext())
            {
                try
                {
                    EfStaticMethods.Log.Info("Checking for expired data in the adi db");
                    var checkWindow = DateTime.Now.AddHours(-Convert.ToInt32(ADIWF_Config.MinusExpiredAssetWindowHours));
                    var expiredRows = CurrentContext.Adi_Data.Where(
                                        item => Convert.ToDateTime(item.Licensing_Window_End.Trim()) < checkWindow);

                    var mapData = new List<GN_Mapping_Data>();
                    //get matching gn row data
                    foreach (var item in expiredRows)
                    {
                        EfStaticMethods.Log.Debug($"DB Row ID {item.Id} with PAID Value: {item.TitlPaid} has expired with License Window End Date: {item.Licensing_Window_End.Trim()} marked for removal.");
                        var adiPaid = EfStaticMethods.GetPaidLastValue(item.TitlPaid);
                        var gnMappingData = CurrentContext.GN_Mapping_Data.FirstOrDefault(p => p.GN_Paid.Contains(adiPaid));
                        if (gnMappingData == null)
                            continue;
                        mapData.Add(gnMappingData);
                    }


                    var rowCount = expiredRows.Count();
                    

                    EfStaticMethods.Log.Info($"Number of expired assets for removal: {expiredRows.Count()}");
                    CurrentContext.Adi_Data.RemoveRange(expiredRows);
                    CurrentContext.GN_Mapping_Data.RemoveRange(mapData);
                    CurrentContext.SaveChanges();


                    EfStaticMethods.Log.Info(rowCount == 0
                        ? "No expired data present."
                        : $"Number of expired assets removed from database = {rowCount}");
                }
                catch (Exception ceaEx)
                {
                    EfStaticMethods.Log.Error($"General Exception during Cleanup of Expired Assets: {ceaEx.Message}");
                    if (ceaEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {ceaEx.InnerException.Message}");
                }

            }

            ExpiryProcessing = false;
        }

        public Adi_Data GetAdiData(string titlPaid)
        {
            using (var db = new ADI_EnrichmentContext())
            {
                var adiPaid = titlPaid
                    .Replace("TITL", "")
                    .Replace("i", "")
                    .TrimStart('0');

                return db.Adi_Data.FirstOrDefault(
                    i => i.TitlPaid.Contains(adiPaid));
            }
        }

        private void CheckAndClearOrphanedData()
        {
            IsOrphanCleanupRunning = true;

            using (CurrentContext = new ADI_EnrichmentContext())
            {
                try
                {
                    /*
                     * See solution Items dir
                     * WORKING SQL QUERY
                     * SELECT id,TITLPAID FROM Adi_Data AS A WHERE NOT EXISTS( SELECT GN_Paid FROM GN_Mapping_Data AS G WHERE RIGHT(G.GN_Paid, 8) = RIGHT(A.TITLPAID, 8))
                     * SELECT id, GN_Paid FROM GN_Mapping_Data AS G WHERE NOT EXISTS ( SELECT TITLPAID FROM Adi_Data AS A WHERE RIGHT(A.TITLPAID,8) = RIGHT(G.GN_Paid,8))
                     */

                    EfStaticMethods.Log.Info("Checking for orphaned db data, this may take time dependent on db size; please be patient");

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();


                    var adiOrphans = CurrentContext.Adi_Data.FromSql("EXEC GetAdiDataOrphans").ToList();
                    if (adiOrphans.Any())
                    {
                        EfStaticMethods.Log.Warn("Adi_Data table has orphaned rows, cleaning up");

                        foreach (var adiO in adiOrphans)
                        {
                            EfStaticMethods.Log.Warn(
                                $"Adi_Data table entry with id: {adiO.Id} and PAID: {adiO.TitlPaid} found that does not exist in GNMapping table, removing row data.");
                            CurrentContext.Database.ExecuteSqlCommand($"DELETE FROM Adi_Data WHERE ID={adiO.Id}");
                        }
                    }


                    var gnOrphans = CurrentContext.GN_Mapping_Data.FromSql("EXEC GetMappingOrphans").ToList();
                    if (gnOrphans.Any())
                    {
                        EfStaticMethods.Log.Warn("GN_Mapping_Data table has orphaned rows, cleaning up");

                        foreach (var gnItem in gnOrphans)
                        {
                            EfStaticMethods.Log.Warn(
                                $"Mapping table entry with id: {gnItem.Id} and PAID: {gnItem.GN_Paid} found that does not exist in adi data table, removing row data.");
                            CurrentContext.Database.ExecuteSqlCommand($"DELETE FROM GN_Mapping_Data WHERE ID={gnItem.Id}");
                        }
                    }

                    stopWatch.Stop();

                    EfStaticMethods.Log.Info($"Orphan cleanup completed in: {stopWatch.Elapsed.Duration()}");
                }
                catch (SqlException sqlEx)
                {
                    EfStaticMethods.Log.Error($"SQL Exception during database connection: {sqlEx.Message}");
                    if (sqlEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {sqlEx.InnerException.Message}");
                }
                catch (Exception capoEx)
                {
                    EfStaticMethods.Log.Error($"General Exception during database connection: {capoEx.Message}");
                    if (capoEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {capoEx.InnerException.Message}");
                }
                finally
                {
                    IsOrphanCleanupRunning = false;
                }
            }

            IsOrphanCleanupRunning = false;
        }
    }
}