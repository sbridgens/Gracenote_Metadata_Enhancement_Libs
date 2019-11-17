using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchTech.File.Manager.Abstract
{
    public interface IAdiArchiveInterface
    {
        bool OperationsSuccessful { get; set; }

        bool HasPreviewAsset { get; set; }

        bool IsUpdatePackage { get; set; }

        FileInfo ExtractedAdiFile { get; set; }

        FileInfo ExtractedMovieAsset { get; set; }

        FileInfo ExtractedPreview { get; set; }

        bool ExtractItemFromArchive(string sourceArchive, string outputDirectory, bool extractAll, bool extractAdiOnly);
        
        bool CreateArchive(string sourceDirectory, string destinationArchive);
    }
}
