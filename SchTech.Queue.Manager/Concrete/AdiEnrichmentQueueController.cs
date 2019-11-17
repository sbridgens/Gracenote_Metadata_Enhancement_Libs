using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Queue.Manager.Abstract;

namespace SchTech.Queue.Manager.Concrete
{
    public class WorkQueueItem
    {
        public FileInfo AdiPackage;
    }
    public class AdiEnrichmentQueueController : IQueueService
    {
        public static ArrayList QueuedPackages { get; set; }

        public AdiEnrichmentQueueController()
        {
            QueuedPackages = new ArrayList();
        }

        public void AddPackageToQueue(FileInfo packageFile)
        {
            bool packageExists = false;

            foreach (WorkQueueItem queItem in QueuedPackages)
            {
                if (queItem.AdiPackage.FullName == packageFile.FullName)
                {
                    packageExists = true;
                    break;
                }
            }

            if (!packageExists)
            {
                WorkQueueItem packageItem = new WorkQueueItem
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
