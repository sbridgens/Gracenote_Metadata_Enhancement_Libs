using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfAdiEnrichmentDal : EfEntityRepositoryBase<Adi_Data, ADI_EnrichmentContext>, IAdiDataDal
    {
        public static bool ExpiryProcessing { get; private set; }

        private ADI_EnrichmentContext CurrentContext { get; set; }

        public bool CleanAdiDataWithNoMapping()
        {
            try
            {
                if (!ExpiryProcessing) Task.Run(ClearExpiredAssets);

                return true;
            }
            catch (SqlException sqlEx)
            {
                EfStaticMethods.Log.Error($"SQL Exception during database connection: {sqlEx.Message}");
                if (sqlEx.InnerException != null)
                    EfStaticMethods.Log.Error($"Inner Exception: {sqlEx.InnerException.Message}");

                return false;
            }
            catch (Exception CAWND_EX)
            {
                EfStaticMethods.Log.Error($"General Exception during database connection: {CAWND_EX.Message}");
                if (CAWND_EX.InnerException != null)
                    EfStaticMethods.Log.Error($"Inner Exception: {CAWND_EX.InnerException.Message}");

                return false;
            }
        }

        private void ClearExpiredAssets()
        {
            ExpiryProcessing = true;
            var expiryCount = 0;

            using (CurrentContext = new ADI_EnrichmentContext())
            {
                EfStaticMethods.Log.Info("Checking for expired data in the adi db");

                var expiredRows = CurrentContext.Adi_Data
                    .Where(item => Convert.ToDateTime(item.Licensing_Window_End.Trim()) < DateTime.Now);

                var mapData = new List<GN_Mapping_Data>();

                foreach (var item in expiredRows)
                {
                    expiryCount++;

                    var adiPaid = EfStaticMethods.GetPaidLastValue(item.TitlPaid);
                    var gnMappingData = CurrentContext.GN_Mapping_Data.FirstOrDefault(p => p.GN_Paid.Contains(adiPaid));
                    mapData.Add(gnMappingData);
                }


                EfStaticMethods.Log.Info($"Number of expired assets for removal: {expiredRows.Count()}");
                CurrentContext.Adi_Data.RemoveRange(expiredRows);
                CurrentContext.GN_Mapping_Data.RemoveRange(mapData);
                CurrentContext.SaveChanges();


                if (expiryCount == 0)
                    EfStaticMethods.Log.Info("No expired data present.");
                else
                    EfStaticMethods.Log.Info($"Number of expired assets removed from database = {expiryCount}");
            }

            CheckAndClearOrphanedData();

            ExpiryProcessing = false;
        }

        private void CheckAndClearOrphanedData()
        {
            using (CurrentContext = new ADI_EnrichmentContext())
            {
                try
                {
                    EfStaticMethods.Log.Info(
                        "Checking for orphaned db data, this may take time dependant on db size; please be patient");

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
                catch (Exception CAPO_EX)
                {
                    EfStaticMethods.Log.Error($"General Exception during database connection: {CAPO_EX.Message}");
                    if (CAPO_EX.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {CAPO_EX.InnerException.Message}");
                }
            }
        }
    }
}