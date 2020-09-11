using System;
using System.Collections.Generic;
using System.IO;

namespace SchTech.Queue.Manager.Abstract
{
    public interface IPollService
    {
        double FailedMappingRepollHours { get; set; }
        bool HasFilesToProcess { get; set; }

        bool IncludeFailedMappingPackages { get; set; }

        DateTime? LastFailedMappingPoll { get; set; }

        string FailedToMapDirectory { get; set; }

        bool StartPollingOperations(string sourcePollDirectory, string fileExtensionToPoll);

        List<FileInfo> WorkflowFileList();
    }
}