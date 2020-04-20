using System;
using ADIWFE_GNTrackerClient.Properties;
using GracenoteUpdateManager;
using log4net;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Parameters;
using SchTech.Configuration.Manager.Schema.GNUpdateTracker;
using SchTech.DataAccess.Concrete.EntityFramework;


namespace ADIWFE_GNTrackerClient
{
    public class GnTrackingOperations
    { 
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(GnTrackingOperations));
        private EfMappingsUpdateTrackingDal UpdateTrackerDal { get; set; }
        private static long LowestUpdateId { get; set; }
        private static long CurrentUpdateId { get; set; }

        public static void LogError(string functionName, string message, Exception ex)
        {
            Log.Error($"[{functionName}] {message}: {ex.Message}");
            if (ex.InnerException != null)
                Log.Error($"[{functionName}] Inner Exception:" +
                          $" {ex.InnerException.Message}");
        }

        public bool LoadAppConfig()
        {

            Log.Info("Loading application configuration");

            try
            {
                var xmlSerializer = new ConfigSerializationHelper<GN_UpdateTracker_Config>();
                if (!xmlSerializer.LoadConfigurationFile(Settings.Default.XmlConfigFile))
                    return false;

                DBConnectionProperties.DbServerOrIp = GN_UpdateTracker_Config.Database_Host;
                DBConnectionProperties.DatabaseName = GN_UpdateTracker_Config.Database_Name;
                DBConnectionProperties.IntegratedSecurity = GN_UpdateTracker_Config.Integrated_Security;

                return true;
            }
            catch (Exception lacEx)
            {
                LogError("LoadAppConfig", "Error Encountered during Config parsing.", lacEx);
                return false;
            }
        }

        public void InitialiseWorkflowOperations()
        {
            UpdateTrackerDal = new EfMappingsUpdateTrackingDal();
        }

        public void StartOperations()
        {
            try
            { 
                GracenoteUpdateController updateController = new GracenoteUpdateController();


                if(CurrentUpdateId==0)
                    LowestUpdateId = Convert.ToInt64(updateController.GetLowestMappingValue(true, false));
                if (CurrentUpdateId != 0)
                {
                    if (LowestUpdateId <= CurrentUpdateId)
                    {
                        LowestUpdateId = ++CurrentUpdateId;
                    }
                }

                Log.Info($"Lowest Mapping UpdateId returned: {LowestUpdateId}");
                updateController.GetGracenoteMappingData(LowestUpdateId.ToString());
                //TODO: Get max update for mappings here!

                Log.Info($"Number of mappings requiring updates is: {updateController.MappingsRequiringUpdate.Count}");

                updateController.GetGracenoteLayer1Updates(LowestUpdateId.ToString());
                Log.Info($"Number of Layer1 Item updates is: {updateController.Layer1DataUpdatesRequiredList.Count}");

                //TODO: Get max update for layer1&2 here as they share the same call!
                updateController.GetGracenoteLayer2Updates(LowestUpdateId.ToString());
                Log.Info($"Number of Layer2 Item updates is: {updateController.Layer2DataUpdatesRequiredList.Count}");
                
                CurrentUpdateId = LowestUpdateId;
                //TODO: Set a max mapping and layer update id, when the iterator is reached then start from the beginning and call db for lowest value.
            }
            catch (Exception spex)
            {
                LogError("StartOperations", "Error during Processing", spex);
            }
        }
    }
}
