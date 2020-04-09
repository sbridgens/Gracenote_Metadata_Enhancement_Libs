using ADIWFE_TestClient.Properties;
using log4net;
using VirginMediaWorkflowDirector;
using SchTech.Configuration.Manager.Concrete;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.File.Manager.Concrete.FileSystem;
using SchTech.Queue.Manager.Concrete;
using System;
using System.Timers;
using SchTech.Configuration.Manager.Parameters;
using SchTech.Entities.ConcreteTypes;

namespace ADIWFE_TestClient
{
    public class AdiWfOperations
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(AdiWfOperations));
        
        private EnrichmentControl WorkflowManager { get; set; }
        private EfAdiEnrichmentDal AdiEnrichmentDal { get; set; }
        private AdiEnrichmentPollController PollController { get; set; }
        private HardwareInformationManager HwInformationManager { get; set; }
        private WorkQueueItem IngestFile { get; set; }
        private bool HasGnMapping { get; set; }
        
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
                var xmlSerializer = new ConfigSerializationHelper();
                if (!ConfigSerializationHelper.LoadConfigurationFile(Settings.Default.XmlConfigFile))
                    return false;

                DBConnectionProperties.DbServerOrIp = ADIWF_Config.Database_Host;
                DBConnectionProperties.DatabaseName = ADIWF_Config.Database_Name;
                DBConnectionProperties.IntegratedSecurity = ADIWF_Config.Integrated_Security;

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
            AdiEnrichmentDal = new EfAdiEnrichmentDal();

            PollController = new AdiEnrichmentPollController
            {
                LastFailedMappingPoll =
                    DateTime.Now.AddHours(-Convert.ToDouble(ADIWF_Config.RepollNonMappedIntervalHours)),
                FailedMappingRepollHours = Convert.ToDouble(ADIWF_Config.RepollNonMappedIntervalHours),
                FailedToMapDirectory = ADIWF_Config.MoveNonMappedDirectory,
                IncludeFailedMappingPackages = ADIWF_Config.ProcessMappingFailures
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
            int failedToMapCount = 0;

            for (var package = 0; package < AdiEnrichmentQueueController.QueuedPackages.Count; package++)
            {
                try
                {
                    IngestFile = (WorkQueueItem)AdiEnrichmentQueueController.QueuedPackages[package];


                    Log.Info(
                        $"############### Processing STARTED For Queued item {package + 1} of {AdiEnrichmentQueueController.QueuedPackages.Count}: {IngestFile.AdiPackage.FullName} ###############\r\n");
                    WorkflowManager = new EnrichmentControl();

                    Success = GetMappingAndExtractPackage();
                    if (!Success)
                        throw new Exception(
                            "Error encountered during GetMappingAndExtractPackage process, check logs and package.");
                    if (!EnrichmentWorkflowEntities.IsMoviePackage)
                    {
                        if(!ProcessSeriesEpisodePackage())
                            throw new Exception("Error Processing Series/Episode data.");
                    }
                    if(AllPackageTasks())
                    {
                        WorkflowManager.PackageCleanup(IngestFile.AdiPackage);
                        AdiEnrichmentQueueController.QueuedPackages.Remove(package);
                        Log.Info(
                            $"############### Processing FINISHED For Queued file: {IngestFile.AdiPackage.Name} ###############\r\n");
                    }
                    else
                    {
                        throw new Exception("Workflow Processing failed.");
                    }
                }
                catch (Exception pqiEx)
                {
                    if(HasGnMapping)
                        LogError("ProcessQueuedItems",
                            $"Error encountered processing package: {IngestFile.AdiPackage.Name}",
                            pqiEx);

                    WorkflowManager.ProcessFailedPackage(IngestFile.AdiPackage);
                    if(HasGnMapping || WorkflowManager.IsPackageAnUpdate)
                        Log.Info($"############### Processing FAILED! for item: {IngestFile.AdiPackage.Name} ###############\r\n");
                    else
                    {
                        Log.Info(
                            $"############### Processing FINISHED For Queued file: {IngestFile.AdiPackage.Name} ###############\r\n");
                    }

                }

                WorkflowManager.CleanStaticReferences();

                if (WorkflowManager.FailedToMap)
                    failedToMapCount++;
            }

            if (failedToMapCount > 0)
                Log.Info($"The number of packages failed to map during this poll was: {failedToMapCount}");
        }

        private bool GetMappingAndExtractPackage()
        {
            try
            {
                if(WorkflowManager.ObtainAndParseAdiFile(IngestFile.AdiPackage) &&
                       WorkflowManager.ValidatePackageIsUnique())
                {
                    HasGnMapping = WorkflowManager.CallAndParseGnMappingData();
                    if(HasGnMapping)
                        return WorkflowManager.SeedGnMappingData() &&
                               WorkflowManager.ExtractPackageMedia() &&
                               WorkflowManager.SetAdiMovieMetadata() &&
                               WorkflowManager.GetGracenoteMovieEpisodeData() &&
                               WorkflowManager.SetAdiMovieEpisodeMetadata();

                    return HasGnMapping;
                }

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

        private bool ProcessSeriesEpisodePackage()
        {
            try
            {
                return WorkflowManager.GetSeriesSeasonSpecialsData() &&
                       WorkflowManager.SetAdiSeriesData() &&
                       WorkflowManager.SetAdiSeasonData();

            }
            catch (Exception pfpex)
            {
                LogError("ProcessFullPackage", "Error Processing Full package", pfpex);
                return false;
            }
        }


        private bool AllPackageTasks()
        {
            try
            {
                return WorkflowManager.ImageSelectionLogic() &&
                          WorkflowManager.RemoveDerivedFromAsset() &&
                          WorkflowManager.FinalisePackageData() &&
                          WorkflowManager.SaveAdiFile() &&
                          WorkflowManager.PackageEnrichedAsset() &&
                          WorkflowManager.DeliverEnrichedAsset();
            }
            catch (Exception aptex)
            {
                LogError("AllPackageTasks", "Error Carrying out all common package tasks", aptex);
                return false;
            }
        }
    }
}