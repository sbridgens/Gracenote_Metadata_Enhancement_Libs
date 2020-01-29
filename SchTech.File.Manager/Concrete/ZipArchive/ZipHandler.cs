using log4net;
using SchTech.File.Manager.Abstract;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SchTech.File.Manager.Concrete.ZipArchive
{
    public class ZipHandler : IDisposable, IAdiArchiveInterface
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZipHandler));
        private FileInfo EntryFileInfo { get; set; }
        private string OutputDirectory { get; set; }
        private bool AdiExtracted { get; set; }
        private bool MovieAssetExtracted { get; set; }
        private bool PreviewExtracted { get; set; }
        private bool PreviewOnly { get; set; }
        public bool OperationsSuccessful { get; set; }
        public bool HasPreviewAsset { get; set; }
        public bool IsUpdatePackage { get; set; }
        public bool IsLegacyGoPackage { get; set; }
        public bool IsTvod { get; set; }
        public FileInfo ExtractedAdiFile { get; set; }
        public FileInfo ExtractedMovieAsset { get; set; }
        public FileInfo ExtractedPreview { get; set; }

        public bool ExtractItemFromArchive(string sourceArchive, string outputDirectory, bool extractAll,
            bool extractAdiOnly)
        {
            OutputDirectory = outputDirectory;
            OperationsSuccessful = false;
            PreviewOnly = false;

            using (var archive = ZipFile.OpenRead(sourceArchive))
            {
                try
                {
                    ValidatePackageEntries(archive);
                    ProcessArchive(archive, extractAdiOnly, IsUpdatePackage);
                   
                    if (extractAdiOnly && AdiExtracted)
                    {
                        OperationsSuccessful = true;
                    }
                    if (IsUpdatePackage && AdiExtracted && !HasPreviewAsset)
                    {
                        OperationsSuccessful = true;
                    }
                    else if (!extractAdiOnly && MovieAssetExtracted)
                    {
                        if (HasPreviewAsset && PreviewExtracted)
                            OperationsSuccessful = true;
                        else if (!HasPreviewAsset)
                            OperationsSuccessful = true;
                    }
                }
                catch (Exception EIFA_Ex)
                {
                    Log.Error($"Error Encountered during extraction of entries from zip package: {EIFA_Ex.Message}");

                    if (EIFA_Ex.InnerException != null) Log.Error($"Inner Exception: {EIFA_Ex.InnerException.Message}");

                    OperationsSuccessful = false;
                }
            }

            return OperationsSuccessful;
        }

        public bool CreateArchive(string sourceDirectory, string destinationArchive)
        {
            try
            {
                Log.Info($"Packaging Source directory: {sourceDirectory} to Zip Archive: {destinationArchive}");
                ZipFile.CreateFromDirectory(sourceDirectory, destinationArchive, CompressionLevel.Fastest, false);
                Log.Info($"Zip Archive: {destinationArchive} created Successfully.");

                OperationsSuccessful = true;
            }
            catch (Exception createZipEx)
            {
                Log.Error($"Failed to Create Zip package: {destinationArchive} - {createZipEx.Message}");

                if (createZipEx.InnerException != null)
                    Log.Error($"Inner Exception: {createZipEx.InnerException.Message}");
                OperationsSuccessful = false;
            }

            return OperationsSuccessful;
        }

        private ZipArchiveEntry ReturnLargestEntry(ZipArchiveEntry zipEntry)
        {
            if (zipEntry.Archive != null)
                foreach (var entry in zipEntry.Archive.Entries.OrderByDescending(e => e.Length))
                    return entry;

            return null;
        }

        private bool ValidateExtraction(long entrySize)
        {
            return EntryFileInfo.Length == entrySize;
        }

        private bool ExtractEntirePackage(string archive)
        {
            try
            {
                ZipFile.ExtractToDirectory(archive, OutputDirectory);
                return true;
            }
            catch (Exception eepEx)
            {
                Log.Error($"Error encountered unpacking source archive: {archive} with exception: {eepEx.Message}");
                return false;
            }
        }

        private void ExtractEntry(ZipArchiveEntry archiveEntry, string entryType)
        {
            var entrySize = archiveEntry.Length;
            var outputFile = IsLegacyGoPackage 
                           ? Path.Combine(OutputDirectory, archiveEntry.FullName)
                           : Path.Combine(OutputDirectory, archiveEntry.Name);


            if (IsLegacyGoPackage && !Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                var dir = Path.GetDirectoryName(outputFile);
                if (dir != null)
                    Directory.CreateDirectory(dir);
            }

            EntryFileInfo = new FileInfo(outputFile);
            CheckWorkingDirectory();
            
            archiveEntry.ExtractToFile(outputFile);

            if (ValidateExtraction(entrySize))
                switch (entryType)
                {
                    case "adi":
                        ExtractedAdiFile = EntryFileInfo;
                        Log.Info($"ADI.xml Successfully extracted to: {outputFile}");
                        AdiExtracted = true;
                        break;
                    case "movie":
                        ExtractedMovieAsset = EntryFileInfo;
                        Log.Info($"Successfully extracted {archiveEntry.Name} to {outputFile}");
                        MovieAssetExtracted = true;
                        break;
                    case "preview":
                        ExtractedPreview = EntryFileInfo;
                        Log.Info($"Successfully extracted {archiveEntry.Name} to {outputFile}");
                        PreviewExtracted = true;
                        break;
                }
        }


        /// <summary>
        ///     Function to iterate archive and trigger extraction of a required file(s)
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="bAdiOnly"></param>
        private void ProcessArchive(System.IO.Compression.ZipArchive archive, bool bAdiOnly,bool bIsUpdate)
        {
            foreach (var entry in archive.Entries.OrderByDescending(e => e.Length))
            {
                if (IsLegacyGoPackage)
                {
                    if (entry.Name.ToLower().Contains("adi"))
                    {
                        ExtractEntry(entry,"adi");
                    }
                    else
                    {
                        ExtractEntry(entry, "movie");
                    }
                }
                else
                {
                    if (!AdiExtracted && entry.Name.ToLower().Equals("adi.xml"))
                    {
                        Log.Info("Extracting ADI File from archive.");
                        ExtractEntry(entry, "adi");
                    }
                    if (bAdiOnly)
                        continue;
                    if (!bIsUpdate)
                    {
                        if (!MovieAssetExtracted && entry.FullName.Contains("media/"))
                        {
                            Log.Info($"Extracting Largest .ts file: {entry.Name} from Package");
                            ExtractEntry(entry, "movie");
                        }
                        if (!PreviewExtracted && entry.FullName.Contains("preview/"))
                        {
                            Log.Info($"Extracting Largest Preview Asset {entry.Name} from Package.");
                            ExtractEntry(entry, "preview");
                        }

                    }
                    else if (PreviewOnly && entry.FullName.Contains("preview/") && !PreviewExtracted)
                    {
                        Log.Info($"Extracting Largest Preview Asset {entry.Name} from Package.");
                        ExtractEntry(entry, "preview");

                    }
                }
            }

           
        }
    

        private void CheckWorkingDirectory()
        {
            if (Directory.Exists(OutputDirectory))
                return;

            Log.Info($"Creating Working Directory: {OutputDirectory}");
            Directory.CreateDirectory(OutputDirectory);
        }

        private void ValidatePackageEntries(System.IO.Compression.ZipArchive archive)
        {
            if (archive.Entries.FirstOrDefault(e => e.FullName.ToLower().Contains("media/")) == null)
            {
                Log.Info("No Media Directory Initially Marking Package as an update");
                IsUpdatePackage = true;
            }

            if (archive.Entries.FirstOrDefault(p => p.FullName.ToLower().Contains("preview/")) != null)
            {
                Log.Info("Package contains a Preview file.");

                if (IsTvod && IsUpdatePackage)
                {
                    Log.Info("TVOD Update package contains a Preview asset for inclusion");
                    PreviewOnly = true;
                }

                HasPreviewAsset = true;
            }
        }

        #region IDisposable

        // Flag: Has Dispose already been called?
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~ZipHandler()
        {
            Dispose(false);
        }

        #endregion
    }
}