using ADIWFE_TestLegacyGo.Properties;
using log4net;
using SchTech.Business.Manager.Concrete.CustomerBusinessLogic.LegacyGo;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.File.Manager.Concrete.FileSystem;
using SchTech.Queue.Manager.Concrete;
using System;
using System.Timers;
using SchTech.Entities.ConcreteTypes;

namespace ADIWFE_TestLegacyGo
{
    public class AdiWfOperations
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(AdiWfOperations));

        private Timer _timer;
        private GoWorkflowManager WorkflowManager { get; set; }
        private EfAdiEnrichmentDal AdiEnrichmentDal { get; set; }
        private AdiEnrichmentPollController PollController { get; set; }
        private HardwareInformationManager HwInformationManager { get; set; }
        private WorkQueueItem IngestFile { get; set; }
        private bool TimerElapsed { get; set; }
        public bool IsInCleanup => false;

        private bool Success { get; set; }

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
                //var ConfigSerializationHelper = new ConfigSerializationHelper();
                return ConfigSerializationHelper.LoadConfigurationFile(Settings.Default.XmlConfigFile);
            }
            catch (Exception lacEx)
            {
                LogError("LoadAppConfig", "Error Encountered during Config parsing.", lacEx);
                return false;
            }
        }
        public void Cleanup()
        {
            if (!IsInCleanup)
            {
                AdiEnrichmentDal.CleanAdiDataWithNoMapping(TimerElapsed);
            }


            TimerElapsed = false;
        }

        /// <summary>
        ///     Timer Event for cleanup, flags a boolean in case there is processing underway
        ///     allowing the clean up to occur post processing
        /// </summary>
        /// <param name="src"></param>
        /// <param name="e"></param>
        private void ElapsedTime(object src, ElapsedEventArgs e)
        {
            TimerElapsed = true;
            if (!IsInCleanup)
            {
                Cleanup();
                TimerElapsed = false;
            }
            else if (EfAdiEnrichmentDal.ExpiryProcessing && !IsInCleanup)
            {
                Log.Info("Cleanup timer elapsed however the service is still flagged as processing.");
                TimerElapsed = true;
            }

            else if (!EfAdiEnrichmentDal.ExpiryProcessing && IsInCleanup)
            {
                Log.Info("Cleanup timer elapsed however the service is currently in a cleanup task.");
                TimerElapsed = true;
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

            PollController = new AdiEnrichmentPollController
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
                    ProcessQueuedItems();
            }
            catch (Exception spex)
            {
                LogError("StartProcessing", "Error during Processing", spex);
            }
        }

        private void ProcessQueuedItems()
        {
            if (AdiEnrichmentQueueController.QueuedPackages?.Count < 1 ||
                AdiEnrichmentQueueController.QueuedPackages == null)
                return;

            EfAdiEnrichmentDal.IsWorkflowProcessing = true;

            for (var package = 0; package < AdiEnrichmentQueueController.QueuedPackages.Count; package++)
            {
                try
                {
                    IngestFile = (WorkQueueItem)AdiEnrichmentQueueController.QueuedPackages[package];


                    Log.Info(
                        $"############### Processing STARTED For Queued item {package + 1} of {AdiEnrichmentQueueController.QueuedPackages.Count}: {IngestFile.AdiPackage.FullName} ###############\r\n");
                    WorkflowManager = new GoWorkflowManager();

                    Success = GetMappingAndExtractPackage();
                    if (!Success)
                        throw new Exception(
                            "Error encountered during GetMappingAndExtractPackage process, check logs and package.");
                    if (!EnrichmentWorkflowEntities.IsMoviePackage)
                        ProcessSeriesEpisodePackage();



                    AllPackageTasks();
                    WorkflowManager.PackageCleanup(IngestFile.AdiPackage);
                    AdiEnrichmentQueueController.QueuedPackages.Remove(package);
                    Log.Info($"############### Processing FINISHED For Queued file: {IngestFile.AdiPackage.Name} ###############\r\n");
                }
                catch (Exception pqiEx)
                {
                    LogError("ProcessQueuedItems",
                        $"Error encountered processing package: {IngestFile.AdiPackage.Name}",
                        pqiEx);

                    WorkflowManager.ProcessFailedPackage(IngestFile.AdiPackage);

                    Log.Info($"############### Processing FAILED! for item: {IngestFile.AdiPackage.Name} ###############\r\n");

                }
            }
        }

        private bool GetMappingAndExtractPackage()
        {
            try
            {
                if (WorkflowManager.ObtainAndParseAdiFile(IngestFile.AdiPackage) &&
                    WorkflowManager.ValidatePackageIsUnique() &&
                    WorkflowManager.CallAndParseGnMappingData() &&
                    WorkflowManager.SeedGnMappingData() &&
                    WorkflowManager.ExtractPackageMedia() &&
                    WorkflowManager.GetGracenoteMovieEpisodeData() &&
                    WorkflowManager.SetAdiMovieEpisodeMetadata())

                    return true;
                return false;
            }
            catch (Exception gmaepEx)
            {
                LogError(
                    "GetMappingAndExtractPackage",
                    "Error encountered during initial mapping check / package extraction",
                    gmaepEx
                );
                return false;
            }
        }

        private void ProcessSeriesEpisodePackage()
        {
            try
            {
                Success = WorkflowManager.GetSeriesSeasonSpecialsData() &&
                          WorkflowManager.SetAdiSeriesData() &&
                          WorkflowManager.SetAdiSeasonData();

            }
            catch (Exception pfpex)
            {
                LogError("ProcessFullPackage", "Error Processing Full package", pfpex);
                Success = false;
            }
        }


        private void AllPackageTasks()
        {
            try
            {
                Success = WorkflowManager.ImageSelectionLogic() &&
                          GoWorkflowManager.RemoveDerivedFromAsset() &&
                          GoWorkflowManager.FinalisePackageData() &&
                          WorkflowManager.SaveAdiFile() &&
                          WorkflowManager.PackageEnrichedAsset(IngestFile.AdiPackage) &&
                          WorkflowManager.DeliverEnrichedAsset();
            }
            catch (Exception aptex)
            {
                LogError("AllPackageTasks", "Error Carrying out all common package tasks", aptex);
            }
        }
    }
}