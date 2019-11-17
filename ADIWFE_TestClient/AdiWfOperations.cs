using System;
using System.Timers;
using SchTech.Business.Manager.Concrete.CustomerBusinessLogic.VirginMedia;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;
using SchTech.File.Manager.Concrete.FileSystem;
using SchTech.Queue.Manager.Concrete;

namespace ADIWFE_TestClient
{
    public class AdiWfOperations
    {
        /// <summary>
        /// Initialize Log4net
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AdiWfOperations));

        private EnrichmentWorkflowManager WorkflowManager { get; set; }

        private EfAdiEnrichmentDal AdiEnrichmentDal { get; set; }
        private AdiEnrichmentPollController PollController { get; set; }
        private HardwareInformationManager HwInformationManager { get; set; }

        private EnrichmentWorkflowEntities WorkflowEntities { get; set; }

        private WorkQueueItem IngestFile { get; set; }

        private Timer _timer;
        public bool IsInCleanup { get; set; }

        private bool Success { get; set; }
        public static void LogError(string functionName, string message, Exception ex)
        {
            Log.Error($"[{functionName}] {message}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Log.Error($"[{functionName}] Inner Exception:" +
                          $" {ex.InnerException.Message}");
            }
        }

        public bool LoadAppConfig()
        {
            Log.Info("Loading application configuration");

            try
            {
                var xmlSerializer = new ConfigSerializationHelper();
                return xmlSerializer.LoadConfigurationFile(Properties.Settings.Default.XmlConfigFile);
            }
            catch (Exception lacEx)
            {
                LogError("LoadAppConfig", "Error Encountered during Config parsing.", lacEx);
                return false;
            }
        }

        /// <summary>
        /// Timer Event for cleanup, flags a boolean in case there is processing underway
        /// allowing the clean up to occur post processing
        /// </summary>
        /// <param name="src"></param>
        /// <param name="e"></param>
        private void ElapsedTime(object src, ElapsedEventArgs e)
        {
            if (!EfAdiEnrichmentDal.ExpiryProcessing)
            {
                AdiEnrichmentDal.CleanAdiDataWithNoMapping();
                //mark as false here as we have tidied up and need to ensure we are not always true.
            }
            else if (!IsInCleanup)
            {
                Log.Info("Cleanup timer elapsed however the service is still flagged as processing.");
            }
            else if (IsInCleanup)
            {
                Log.Info("Cleanup timer elapsed however the service is currently in a cleanup task.");
            }
        }

        private void InitialiseTimer()
        {
            _timer = new Timer
            {
                Interval = Convert.ToDouble(ADIWF_Config.ExpiredAssetCleanupIntervalHours) * 60 * 60 * 1000
            };
            _timer.Elapsed += ElapsedTime;
            _timer.Start();
        }

        public void InitialiseWorkflowOperations()
        {
            AdiEnrichmentDal = new EfAdiEnrichmentDal();
            InitialiseTimer();


            if (WorkflowManager == null)
                WorkflowManager = new EnrichmentWorkflowManager();

            //initial check for orphaned data in the db
            WorkflowManager.CheckAndCleanOrphanedData();

            PollController = new AdiEnrichmentPollController()
            {
                LastFailedMappingPoll =
                    DateTime.Now.AddHours(-Convert.ToDouble(ADIWF_Config.RepollNonMappedIntervalHours)),
                FailedMappingRepollHours = Convert.ToDouble(ADIWF_Config.RepollNonMappedIntervalHours),
                FailedToMapDirectory = ADIWF_Config.MoveNonMappedDirectory

            };
        }

        private bool CanProcess()
        {
            HwInformationManager = new HardwareInformationManager();
            AdiWfManager.IsRunning = HwInformationManager.GetDriveSpace();
            return AdiWfManager.IsRunning;

        }

        public void StartProcessing()
        {
            try
            {
                if (!CanProcess())
                    return;

                if (PollController.StartPollingOperations(ADIWF_Config.InputDirectory, "*.zip"))
                {
                    ProcessQueuedItems();
                }

            }
            catch (Exception spex)
            {
                LogError("StartProcessing","Error during Processing", spex);
            }
        }

        private void ProcessQueuedItems()
        {
            if (AdiEnrichmentQueueController.QueuedPackages?.Count < 1 ||
                AdiEnrichmentQueueController.QueuedPackages == null)
                return;

            for (int package = 0; package < AdiEnrichmentQueueController.QueuedPackages.Count; package++)
            {
                try
                {
                    WorkflowEntities = new EnrichmentWorkflowEntities();
                    IngestFile = (WorkQueueItem) AdiEnrichmentQueueController.QueuedPackages[package];


                    Log.Info($"############### Processing STARTED For Queued item {package + 1} of {AdiEnrichmentQueueController.QueuedPackages.Count}: {IngestFile.AdiPackage.FullName} ###############\r\n");

                    Success = GetMappingAndExtractPackage();
                    if (!Success)
                    {
                        throw new Exception("Error encountered during GetMappingAndExtractPackage process, check logs and package.");
                    }
                    if (!WorkflowEntities.IsPackageAnUpdate)
                    {
                        Success = ProcessFullPackage();
                    }

                    if (WorkflowEntities.IsPackageAnUpdate)
                    {
                        Success = ProcessUpdatePackage();
                    }

                    if (WorkflowEntities.IsPackageTvod)
                    {
                        Success = ProcessTvodPackage();
                    }

                    WorkflowManager.ImageSelectionLogic();
                    WorkflowEntities.SaveAdiFile();
                }
                catch (Exception pqiEx)
                {
                    LogError("ProcessQueuedItems",
                        $"Error encountered processing package: {IngestFile.AdiPackage.Name}", 
                        pqiEx);

                    WorkflowManager.ProcessFailedPackage();

                }
            }
        }

        private bool GetMappingAndExtractPackage()
        {
            try
            {

                if (WorkflowManager.ObtainAndParseAdiFile(IngestFile.AdiPackage) &&
                    WorkflowManager.CallAndParseGnMappingData() &&
                    WorkflowManager.ValidatePackageIsUnique() &&
                    WorkflowManager.SeedGnMappingData() && 
                    WorkflowManager.ExtractPackageMedia() &&
                    WorkflowManager.SetAdiMovieMetadata() &&
                    WorkflowManager.CheckAndAddPreviewData())
                    return true;

                throw new Exception();
            }
            catch (Exception gmaeadiEx)
            {
                LogError(
                    "GetMappingAndExtractPackage",
                    "Error encountered during initial mapping check / package extraction", 
                    ex: gmaeadiEx
                    );
                return false;
            }
        }

        private bool ProcessFullPackage()
        {
            try
            {
                Success = WorkflowManager.GetGracenoteMovieEpisodeData() &&
                          WorkflowManager.GetSeriesSeasonSpecialsData() &&
                          WorkflowManager.SetAdiEpisodeMetadata();
                if (Success)
                {
                    if (!WorkflowEntities.IsMoviePackage)
                    {
                        return WorkflowManager.SetAdiSeriesData() &&
                               WorkflowManager.SetAdiSeasonData() &&
                               WorkflowManager.RemoveDerivedFromAsset();
                    }
                }

            }
            catch (Exception pfpex)
            {
                LogError("ProcessFullPackage","Error Processing Full package", pfpex);
            }
            throw new NotImplementedException();
        }

        private bool ProcessUpdatePackage()
        {
            throw new NotImplementedException();
        }

        private bool ProcessTvodPackage()
        {
            throw new NotImplementedException();
        }
    }

}
