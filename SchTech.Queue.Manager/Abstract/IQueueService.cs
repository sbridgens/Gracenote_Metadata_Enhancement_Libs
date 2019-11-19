using System.IO;

namespace SchTech.Queue.Manager.Abstract
{
    public interface IQueueService
    {
        void AddPackageToQueue(FileInfo packageFile);

        void ClearWorkQueue();
    }
}