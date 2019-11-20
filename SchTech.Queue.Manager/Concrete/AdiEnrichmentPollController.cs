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

        private List<FileInfo> PackageList;

        private string _sourcePollDirectory { get; set; }

        private string _fileExtensionToPoll { get; set; }

        private AdiEnrichmentQueueController WorkQueue { get; set; }

        private int packageCount { get; set; }

        public double FailedMappingRepollHours { get; set; }
        public bool HasFilesToProcess { get; set; }
        public bool IncludeFailedMappingPackages { get; set; }
        public DateTime? LastFailedMappingPoll { get; set; }
        public string FailedToMapDirectory { get; set; }

        public bool StartPollingOperations(string sourcePollDirectory, string fileExtensionToPoll)
        {
            HasFilesToProcess = false;
            WorkQueue = new AdiEnrichmentQueueController();
            WorkQueue.ClearWorkQueue();
            IncludeFailedMappingPackages = false;
            _sourcePollDirectory = sourcePollDirectory;
            _fileExtensionToPoll = fileExtensionToPoll;

            Log.Info($"Polling Input Directory: {sourcePollDirectory} for Packages");

            if (InputDirExists())
            {
                var dtnow = DtNow();

                if (dtnow >= LastFailedMappingPoll) SetFailedMappingPollTime();


                if (WorkflowFileList().Count >= 1)
                {
                    foreach (var package in PackageList) WorkQueue.AddPackageToQueue(package);

                    HasFilesToProcess = true;
                }

                return true;
            }

            Log.Error($"Input Directory: {_sourcePollDirectory} Does not Exist!");
            return false;
        }

        public List<FileInfo> WorkflowFileList()
        {
            PackageList = new List<FileInfo>();

            try
            {
                BuildPackageList();
                if (IncludeFailedMappingPackages)
                    AddMappingFailuresToList();

                if (packageCount >= 1)
                    Log.Info($"Number of Packages added to the Work queue for Processing: {packageCount}\r\n\r\n");
            }
            catch (Exception WFL_EX)
            {
                Log.Error(
                    $"[AdiEnrichmentPollController] Error encountered during poll list creation: {WFL_EX.Message}");

                if (WFL_EX.InnerException != null)
                    Log.Error($"[AdiEnrichmentPollController] Inner Exception: {WFL_EX.InnerException.Message}");
            }

            return PackageList;
        }

        private DateTime? DtNow()
        {
            return DateTime.Now;
        }

        private bool InputDirExists()
        {
            return Directory.Exists(_sourcePollDirectory);
        }

        private void SetFailedMappingPollTime()
        {
            IncludeFailedMappingPackages = true;
            LastFailedMappingPoll = DateTime.Now.AddHours(FailedMappingRepollHours);
        }

        private void BuildPackageList()
        {
            packageCount = 0;
            var directoryInfo = new DirectoryInfo(_sourcePollDirectory);

            foreach (var adiPackage in
                directoryInfo.GetFiles(_fileExtensionToPoll,
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                packageCount++;
                Log.Info($"Adding File: {adiPackage.FullName} to the Work Queue");
                PackageList.Add(adiPackage);
            }
        }

        private void AddMappingFailuresToList()
        {
            var fMapDinfo = new DirectoryInfo(FailedToMapDirectory);

            foreach (var mapFailure in
                fMapDinfo.GetFiles(_fileExtensionToPoll,
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                packageCount++;
                Log.Info($"Adding Previously Failed to map Package: {mapFailure.FullName} to the Work Queue");
                PackageList.Add(mapFailure);
            }
        }
    }
}