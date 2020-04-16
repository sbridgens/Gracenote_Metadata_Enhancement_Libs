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

               // var lowest = updateController.GetLowestMappingValue(true, false);
               // Log.Info($"Lowest Mapping UpdateId returned: {lowest}");
               // updateController.GetGracenoteMappingData(lowest);

                Log.Info($"Number of mappings requiring updates is: {updateController.MappingsRequiringUpdate.Count}");

            }
            catch (Exception spex)
            {
                LogError("StartOperations", "Error during Processing", spex);
            }
        }
    }
}
