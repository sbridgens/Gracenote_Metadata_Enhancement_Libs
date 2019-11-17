using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchTech.Queue.Manager.Abstract
{
    public interface IQueueService
    {
        void AddPackageToQueue(FileInfo packageFile);

        void ClearWorkQueue();
    }
}
