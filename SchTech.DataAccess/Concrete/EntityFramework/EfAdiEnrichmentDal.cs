using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfAdiEnrichmentDal : EfEntityRepositoryBase<Adi_Data, ADI_EnrichmentContext>, IAdiDataDal
    {
        public static bool ExpiryProcessing { get; private set; }

        public static bool IsWorkflowProcessing { get; set; }
        
        private ADI_EnrichmentContext CurrentContext { get; set; }

        public async Task<bool> CheckAndClearExpiredData(bool timerElapsed)
        {
            try
            {
                if (!ExpiryProcessing && timerElapsed)
                {
                    await Task.Run((Action)ClearExpiredAssets);
                }

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

        private async void ClearExpiredAssets()
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
                        var gnMappingData = CurrentContext.GN_Mapping_Data.FirstOrDefault(p => p.IngestUUID == item.IngestUUID);

                        if (gnMappingData == null)
                            continue;
                        mapData.Add(gnMappingData);
                    }
                    
                    var rowCount = expiredRows.Count();
                    

                    EfStaticMethods.Log.Info($"Number of expired assets for removal: {expiredRows.Count()}");
                    CurrentContext.GN_Mapping_Data.RemoveRange(mapData);

                    EfStaticMethods.Log.Info(rowCount == 0
                        ? "No expired data present."
                        : $"Number of expired assets removed from database = {rowCount}");


                    ExpiryProcessing = false;

                    await CurrentContext.SaveChangesAsync();


                }
                catch (Exception ceaEx)
                {

                    ExpiryProcessing = false;
                    EfStaticMethods.Log.Error($"General Exception during Cleanup of Expired Assets: {ceaEx.Message}");
                    if (ceaEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {ceaEx.InnerException.Message}");
                }
            }
        }

        public Adi_Data GetAdiData(Guid adiGuid)
        {
            using (var db = new ADI_EnrichmentContext())
            {
               return db.Adi_Data.FirstOrDefault(i => i.IngestUUID.Equals(adiGuid));
            }
        }
    }
}