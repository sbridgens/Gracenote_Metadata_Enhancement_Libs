using SchTech.Queue.Manager.Abstract;
using System.Collections;
using System.IO;

namespace SchTech.Queue.Manager.Concrete
{
    public class WorkQueueItem
    {
        public FileInfo AdiPackage;
    }

    public class AdiEnrichmentQueueController : IQueueService
    {
        public AdiEnrichmentQueueController()
        {
            QueuedPackages = new ArrayList();
        }

        public static ArrayList QueuedPackages { get; set; }

        public void AddPackageToQueue(FileInfo packageFile)
        {
            var packageExists = false;

            foreach (WorkQueueItem queItem in QueuedPackages)
                if (queItem.AdiPackage.FullName == packageFile.FullName)
                {
                    packageExists = true;
                    break;
                }

            if (!packageExists)
            {
                var packageItem = new WorkQueueItem
                {
                    AdiPackage = packageFile
                };

                QueuedPackages.Add(packageItem);
            }
        }

        public void ClearWorkQueue()
        {
            QueuedPackages.Clear();
        }
    }
}