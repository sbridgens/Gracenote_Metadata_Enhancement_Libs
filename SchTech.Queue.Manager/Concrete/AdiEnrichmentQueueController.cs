using SchTech.Queue.Manager.Abstract;
using System.Collections;
using System.IO;
using System.Linq;

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

        public static ArrayList QueuedPackages { get; private set; }

        public void AddPackageToQueue(FileInfo packageFile)
        {
            var packageExists = QueuedPackages.Cast<WorkQueueItem>().Any(
                queItem => queItem.AdiPackage.FullName == packageFile.FullName);

            if (packageExists)
                return;

            var packageItem = new WorkQueueItem
            {
                AdiPackage = packageFile
            };

            QueuedPackages.Add(packageItem);
        }

        public void ClearWorkQueue()
        {
            QueuedPackages.Clear();
        }
    }
}