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

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfAdiEnrichmentDal : EfEntityRepositoryBase<Adi_Data, ADI_EnrichmentContext>, IAdiDataDal
    {
        public static bool ExpiryProcessing { get; private set; }

        public static bool IsWorkflowProcessing { get; set; }

        private ADI_EnrichmentContext CurrentContext { get; set; }

        public bool CleanAdiDataWithNoMapping()
        {
            try
            {
                if (!IsWorkflowProcessing)
                    CheckAndClearOrphanedData();
                if (!ExpiryProcessing)
                    Task.Run(ClearExpiredAssets);

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
            var expiryCount = 0;

            using (CurrentContext = new ADI_EnrichmentContext())
            {
                try
                {
                    EfStaticMethods.Log.Info("Checking for expired data in the adi db");
                    var checkWindow = Convert.ToInt32(ADIWF_Config.MinusExpiredAssetWindowHours);
                    var expiredRows = CurrentContext.Adi_Data
                        .Where(item => Convert.ToDateTime(item.Licensing_Window_End.Trim()) < DateTime.Now.AddHours(-checkWindow)).ToList();

                    var mapData = new List<GN_Mapping_Data>();

                    foreach (var item in expiredRows)
                    {
                        expiryCount++;

                        var adiPaid = EfStaticMethods.GetPaidLastValue(item.TitlPaid);
                        var gnMappingData = CurrentContext.GN_Mapping_Data.FirstOrDefault(p => p.GN_Paid.Contains(adiPaid));
                        if (gnMappingData == null)
                            continue;

                        EfStaticMethods.Log.Debug($"DB Row ID {item.Id} with PAID Value: {item.TitlPaid} " +
                                                  $"has expired with License Window End Date: {item.Licensing_Window_End.Trim()}, " +
                                                  $"marked for removal.");

                        mapData.Add(gnMappingData);
                    }


                    EfStaticMethods.Log.Info($"Number of expired assets for removal: {expiredRows.Count()}");
                    CurrentContext.Adi_Data.RemoveRange(expiredRows);
                    CurrentContext.GN_Mapping_Data.RemoveRange(mapData);
                    CurrentContext.SaveChanges();


                    EfStaticMethods.Log.Info(expiryCount == 0
                        ? "No expired data present."
                        : $"Number of expired assets removed from database = {expiryCount}");
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
            using (CurrentContext = new ADI_EnrichmentContext())
            {
                try
                {
                    EfStaticMethods.Log.Info(
                        "Checking for orphaned db data, this may take time dependent on db size; please be patient");

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    var adiNoMapped = CurrentContext.Adi_Data.Select(a => new
                    {
                        a.Id,
                        a.TitlPaid
                    }).ToList();

                    var mappedNoAdi = CurrentContext.GN_Mapping_Data.Select(m => new
                    {
                        m.Id,
                        m.GN_Paid
                    }).ToList();

                    var logcount = 0;

                    foreach (var item in adiNoMapped)
                    {
                        var a = EfStaticMethods.GetPaidLastValue(item.TitlPaid);
                        if (mappedNoAdi.Any(i => i.GN_Paid.Contains(a)))
                            continue;

                        var adidata = CurrentContext.Adi_Data.FirstOrDefault(i => i.Id == item.Id);

                        if (adidata == null) continue;

                        if (logcount == 0)
                        {
                            EfStaticMethods.Log.Warn("ADI Data table has orphaned rows, cleaning up");
                            logcount++;
                        }

                        EfStaticMethods.Log.Warn(
                            $"Adi_Data table entry with id: {item.Id} and PAID: {item.TitlPaid} found that does not exist in GNMapping table, removing row data.");

                        CurrentContext.Adi_Data.Remove(adidata);
                    }


                    if (logcount > 0)
                    {
                        CurrentContext.SaveChanges();
                        logcount = 0;
                        //changes made so update lists.
                        adiNoMapped = CurrentContext.Adi_Data.Select(a => new
                        {
                            a.Id,
                            a.TitlPaid
                        }).ToList();


                        mappedNoAdi = CurrentContext.GN_Mapping_Data.Select(m => new
                        {
                            m.Id,
                            m.GN_Paid
                        }).ToList();
                    }


                    foreach (var item in mappedNoAdi)
                    {
                        var m = EfStaticMethods.GetPaidLastValue(item.GN_Paid);
                        if (adiNoMapped.Any(i => i.TitlPaid.Contains(m)))
                            continue;

                        var mapdata = CurrentContext.GN_Mapping_Data.FirstOrDefault(i => i.Id == item.Id);
                        if (mapdata == null)
                            continue;

                        if (logcount == 0)
                        {
                            EfStaticMethods.Log.Warn("GN_Mapping_Data has orphaned rows, cleaning up");
                            logcount++;
                        }

                        EfStaticMethods.Log.Warn(
                            $"Mapping table entry with id: {item.Id} and PAID: {item.GN_Paid} found that does not exist in adi data table, removing row data.");

                        CurrentContext.GN_Mapping_Data.Remove(mapdata);
                    }

                    CurrentContext.SaveChanges();
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
            }
        }
    }
}