using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Queue.Manager.Abstract;

namespace SchTech.Queue.Manager.Concrete
{
    public class AdiEnrichmentPollController : IPollService
    {
        /// <summary>
        /// Initialize Log4net
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AdiEnrichmentPollController));

        public double FailedMappingRepollHours { get; set; }
        public bool HasFilesToProcess { get; set; }
        public bool IncludeFailedMappingPackages { get; set; }
        public DateTime? LastFailedMappingPoll { get; set; }
        public string FailedToMapDirectory { get; set; }

        private string _sourcePollDirectory { get; set; }

        private string _fileExtensionToPoll { get; set; }

        private AdiEnrichmentQueueController WorkQueue { get; set; }

        private int packageCount { get; set; }

        private List<FileInfo> PackageList;

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

        public bool StartPollingOperations(string sourcePollDirectory, string fileExtensionToPoll)
        {
            HasFilesToProcess = false;
            IncludeFailedMappingPackages = false;
            this._sourcePollDirectory = sourcePollDirectory;
            this._fileExtensionToPoll = fileExtensionToPoll;

            Log.Info($"Polling Input Directory: {sourcePollDirectory} for Packages");

            if (InputDirExists())
            {
                var dtnow = DtNow();

                if (dtnow >= LastFailedMappingPoll)
                {
                    SetFailedMappingPollTime();
                }


                if (WorkflowFileList().Count >= 1)
                {
                    WorkQueue = new AdiEnrichmentQueueController();
                    WorkQueue.ClearWorkQueue();
                    foreach (FileInfo package in PackageList)
                    {
                        WorkQueue.AddPackageToQueue(package);
                    }

                    HasFilesToProcess = true;
                }

                return true;
            }
            else
            {
                Log.Error($"Input Directory: {_sourcePollDirectory} Does not Exist!");
                return false;
            }
        }

        private void BuildPackageList()
        {
            packageCount = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(_sourcePollDirectory);

            foreach (var adiPackage in 
                directoryInfo.GetFiles(_fileExtensionToPoll,
                    searchOption:SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                packageCount++;
                Log.Info($"Adding File: {adiPackage.FullName} to the Work Queue");
                PackageList.Add(adiPackage);
            }
        }

        private void AddMappingFailuresToList()
        {
            DirectoryInfo fMapDinfo = new DirectoryInfo(FailedToMapDirectory);

            foreach (FileInfo mapFailure in 
                fMapDinfo.GetFiles(_fileExtensionToPoll, 
                    searchOption: SearchOption.TopDirectoryOnly)
                    .OrderBy(ct => ct.CreationTime).ToArray())
            {
                packageCount++;
                Log.Info($"Adding Previously Failed to map Package: {mapFailure.FullName} to the Work Queue");
                PackageList.Add(mapFailure);
            }
        }

        public List<FileInfo> WorkflowFileList()
        {
            PackageList = new List<FileInfo>();

            try
            {
                BuildPackageList();
                if (IncludeFailedMappingPackages)
                {
                    AddMappingFailuresToList();
                }

                if (packageCount >= 1)
                {
                    Log.Info($"Number of Packages added to the Work queue for Processing: {packageCount}\r\n\r\n");
                }
            }
            catch (Exception WFL_EX)
            {
                Log.Error($"[AdiEnrichmentPollController] Error encountered during poll list creation: {WFL_EX.Message}");

                if (WFL_EX.InnerException != null)
                {
                    Log.Error($"[AdiEnrichmentPollController] Inner Exception: {WFL_EX.InnerException.Message}");
                }
            }

            return PackageList;
        }

    }
}
