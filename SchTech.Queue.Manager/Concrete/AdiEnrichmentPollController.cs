using log4net;
using SchTech.Queue.Manager.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SchTech.Queue.Manager.Concrete
{
    public class AdiEnrichmentPollController : IPollService
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(AdiEnrichmentPollController));

        private List<FileInfo> _packageList;

        private string SourcePollDirectory { get; set; }

        private string FileExtensionToPoll { get; set; }

        private AdiEnrichmentQueueController WorkQueue { get; set; }

        private int PackageCount { get; set; }

        public double FailedMappingRepollHours { get; set; }
        public bool HasFilesToProcess { get; set; }
        public bool IncludeFailedMappingPackages { get; set; }
        public DateTime? LastFailedMappingPoll { get; set; }
        public string FailedToMapDirectory { get; set; }
        private bool ProcessMappingFailures { get; set; }

        public bool StartPollingOperations(string sourcePollDirectory, string fileExtensionToPoll)
        {
            HasFilesToProcess = false;
            WorkQueue = new AdiEnrichmentQueueController();
            WorkQueue.ClearWorkQueue();
            SourcePollDirectory = sourcePollDirectory;
            FileExtensionToPoll = fileExtensionToPoll;

            Log.Info($"Polling Input Directory: {sourcePollDirectory} for Packages");

            if (InputDirExists())
            {
                var dtnow = DtNow();

                if (dtnow >= LastFailedMappingPoll)
                {
                    SetFailedMappingPollTime();
                    ProcessMappingFailures = true;
                }
                
                if (WorkflowFileList().Count < 1)
                    return true;

                foreach (var package in _packageList) WorkQueue.AddPackageToQueue(package);

                HasFilesToProcess = true;

                return true;
            }

            Log.Error($"Input Directory: {SourcePollDirectory} Does not Exist!");
            return false;
        }

        public List<FileInfo> WorkflowFileList()
        {
            _packageList = new List<FileInfo>();

            try
            {
                BuildPackageList();
                if (IncludeFailedMappingPackages & ProcessMappingFailures)
                    AddMappingFailuresToList();

                if (PackageCount >= 1)
                    Log.Info($"Number of Packages added to the Work queue for Processing: {PackageCount}\r\n\r\n");
            }
            catch (Exception wflEx)
            {
                Log.Error(
                    $"[AdiEnrichmentPollController] Error encountered during poll list creation: {wflEx.Message}");

                if (wflEx.InnerException != null)
                    Log.Error($"[AdiEnrichmentPollController] Inner Exception: {wflEx.InnerException.Message}");
            }

            return _packageList;
        }

        private static DateTime? DtNow()
        {
            return DateTime.Now;
        }

        private bool InputDirExists()
        {
            return Directory.Exists(SourcePollDirectory);
        }

        private void SetFailedMappingPollTime()
        {
            LastFailedMappingPoll = DateTime.Now.AddHours(FailedMappingRepollHours);
        }

        private void BuildPackageList()
        {
            PackageCount = 0;
            var directoryInfo = new DirectoryInfo(SourcePollDirectory);

            foreach (var adiPackage in
                directoryInfo.GetFiles(FileExtensionToPoll,
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                PackageCount++;
                Log.Info($"Adding File: {adiPackage.FullName} to the Work Queue");
                _packageList.Add(adiPackage);
            }
        }

        private void AddMappingFailuresToList()
        {
            var fMapDinfo = new DirectoryInfo(FailedToMapDirectory);

            foreach (var mapFailure in
                fMapDinfo.GetFiles(FileExtensionToPoll,
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                PackageCount++;
                Log.Info($"Adding Previously Failed to map Package: {mapFailure.FullName} to the Work Queue");
                _packageList.Add(mapFailure);
            }

            ProcessMappingFailures = false;
        }
    }
}