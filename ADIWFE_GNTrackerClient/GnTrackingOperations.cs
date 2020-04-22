using System;
using ADIWFE_GNTrackerClient.Properties;
using GracenoteUpdateManager;
using log4net;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Parameters;
using SchTech.Configuration.Manager.Schema.GNUpdateTracker;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;


namespace ADIWFE_GNTrackerClient
{
    public class GnTrackingOperations
    { 
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(GnTrackingOperations));
        private EfMappingsUpdateTrackingDal UpdateTrackerDal { get; set; }
        private EfLatestUpdateIdsDal UpdateIdsDal { get; set; }
        private GracenoteUpdateController UpdateController { get; set; }

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
            UpdateIdsDal = new EfLatestUpdateIdsDal();
        }

        public void StartOperations()
        {
            try
            { 
                UpdateController = new GracenoteUpdateController();

                if (CheckAndProcessMappingUpdates())
                {
                    Log.Info("Mapping updates check successful");
                }

            }
            catch (Exception spex)
            {
                LogError("StartOperations", "Error during Processing", spex);
            }
        }

        private bool CheckAndProcessMappingUpdates()
        {
            try
            {
                //initialise lowestupdateid
                var lowestUpdateId = GracenoteUpdateController.NextMappingUpdateId;
                //checks the last updateid checked in the LatestUpdateIds db table
                var mapId = UpdateIdsDal.Get(v => v.LastMappingUpdateIdChecked != 0);

                //if no next updateid then obtain values from db
                if (lowestUpdateId == 0)
                {
                    //Get lowest updateId checks tracking first then fallsback to mapping if null
                    lowestUpdateId = UpdateController.GetLowestMappingUpdateId();

                    if(lowestUpdateId < 1)
                    {
                        //Log the entry being created
                        Log.Info(
                            $"No entry found in the LatestUpdateIds db table, adding new row with Mapping Update Id: {lowestUpdateId} and 0 for the Layer1/2 columns" +
                            $" these will update during workflow.");
                        mapId = new LatestUpdateIds
                        {
                            //use the lowest updateid
                            LastMappingUpdateIdChecked = lowestUpdateId,
                            //set to 0 for initialisation
                            LastLayer1UpdateIdChecked = 0,
                            LastLayer2UpdateIdChecked = 0
                        };
                        //add row to table
                        UpdateIdsDal.Add(mapId);
                    }
                }
                //Check in order to log if we have reached max update id.

                CheckMaxUpdates(lowestUpdateId);
                //Log the lowest update id
                Log.Info($"Mapping UpdateId being used for Updates Call to Gracenote: {lowestUpdateId}");
                //Call the GN OnApi function to retrieve the Mapping updates.
                UpdateController.GetGracenoteMappingData(lowestUpdateId.ToString());

                Log.Info(
                    $"Number of mappings requiring updates is: {UpdateController.MappingsRequiringUpdate.Count}");
                return true;
            }
            catch (Exception capmuEx)
            {
                LogError("CheckAndProcessMappingUpdates", "Error during Parsing of Mapping Updates", capmuEx);
                return false;
            }
        }

        private void CheckMaxUpdates(long lowestUpdateId)
        {
            try
            {
                //Check if the current updateid is equal to the max updateid and log
                if (lowestUpdateId == GracenoteUpdateController.MaxMappingUpdateId)
                {
                    Log.Info($"Workflow has reached the Maximum Gracenote Mapping UpdateId: {GracenoteUpdateController.MaxMappingUpdateId}");
                    Log.Info($"Continuing to check if a next update id is available and if there are updates including in this id?");
                }
            }
            catch (Exception cmuException)
            {
                LogError("CheckMaxUpdates", "Error While carrying out Max Updates Check", cmuException);
            }
        }
    }
}
