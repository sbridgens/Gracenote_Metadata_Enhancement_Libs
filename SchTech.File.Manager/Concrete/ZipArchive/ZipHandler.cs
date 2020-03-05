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
        #region Properties

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

        #endregion Properties


        private void CheckWorkingDirectory()
        {
            if (Directory.Exists(OutputDirectory))
                return;

            Log.Info($"Creating Working Directory: {OutputDirectory}");
            Directory.CreateDirectory(OutputDirectory);
        }


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

                    if (extractAdiOnly || IsLegacyGoPackage && AdiExtracted)
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
                catch (Exception eifaEx)
                {
                    Log.Error($"Error Encountered during extraction of entries from zip package: {eifaEx.Message}");

                    if (eifaEx.InnerException != null) Log.Error($"Inner Exception: {eifaEx.InnerException.Message}");

                    OperationsSuccessful = false;
                }
            }

            return OperationsSuccessful;
        }



        private void ValidatePackageEntries(System.IO.Compression.ZipArchive archive)
        {
            if (archive.Entries.FirstOrDefault(e => e.FullName.ToLower().Contains("media/")) == null)
            {
                Log.Info("No Media Directory Initially Marking Package as an update");
                IsUpdatePackage = true;
            }

            if (archive.Entries.FirstOrDefault(p => p.FullName.ToLower().Contains("preview/")) == null)
                return;

            Log.Info("Package contains a Preview file.");

            if (IsTvod && IsUpdatePackage)
            {
                Log.Info("TVOD Update package contains a Preview asset for inclusion");
                PreviewOnly = true;
            }

            HasPreviewAsset = true;
        }

        /// <summary>
        ///     Function to iterate archive and trigger extraction of a required file(s)
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="bAdiOnly"></param>
        /// <param name="bIsUpdate"></param>
        private void ProcessArchive(System.IO.Compression.ZipArchive archive, bool bAdiOnly, bool bIsUpdate)
        {
            foreach (var entry in archive.Entries.OrderByDescending(e => e.Length))
            {
                if (AdiExtracted && IsLegacyGoPackage)
                {
                    bAdiOnly = true;
                    break;
                }

                if (!AdiExtracted && IsLegacyGoPackage && entry.Name.ToLower().Equals("adi.xml"))
                {
                    Log.Info("Legacy go Package - Extracting ADI.xml Only");
                    ExtractEntry(entry, "adi");
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

                        if (PreviewExtracted || !entry.FullName.Contains("preview/"))
                            continue;

                        Log.Info($"Extracting Largest Preview Asset {entry.Name} from Package.");
                        ExtractEntry(entry, "preview");

                    }
                    else if (PreviewOnly && entry.FullName.Contains("preview/") && !PreviewExtracted)
                    {
                        Log.Info($"Extracting Largest Preview Asset {entry.Name} from Package.");
                        ExtractEntry(entry, "preview");

                    }
                }
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
                {
                    Directory.CreateDirectory(dir);

                    Log.Info($"Created Legacy go Output Directory: {dir}");
                }
            }

            EntryFileInfo = new FileInfo(outputFile);
            CheckWorkingDirectory();

            archiveEntry.ExtractToFile(outputFile, true);

            if (!ValidateExtraction(entrySize))
                return;
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


        private bool ValidateExtraction(long entrySize)
        {
            return EntryFileInfo.Length == entrySize;
        }


        public static void DeletePostFromPackage(string sourceArchive)
        {
            try
            {
                var patterns = new[] { ".jpg", ".jpeg", ".gif", ".png", ".bmp" };

                using (var zipArchive = ZipFile.Open(sourceArchive, ZipArchiveMode.Update))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        foreach (var ext in patterns)
                        {
                            if (!Path.GetExtension(entry.Name).Equals(ext))
                                continue;

                            var archiveFile = zipArchive.GetEntry(entry.Name);
                            archiveFile?.Delete();
                        }
                    }
                }

            }
            catch (Exception deletePosterEx)
            {
                Log.Error($"Failed to Deleting Poster from Archive: {sourceArchive} - {deletePosterEx.Message}");

                if (deletePosterEx.InnerException != null)
                    Log.Error($"Inner Exception: {deletePosterEx.InnerException.Message}");

            }
        }

       
        public bool CreateLegacyGoPackage(string sourceArchive, string sourceDirectory)
        {
            try
            {
                var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(sourceArchive);

                zipFile.BeginUpdate();

                foreach (var file in Directory.EnumerateFiles(
                    sourceDirectory,
                    "*.*",
                    searchOption: SearchOption.TopDirectoryOnly))
                {
                    var fileInfo = new FileInfo(file);
                    Log.Info($"Adding File: {fileInfo.Name} to Package");

                    zipFile.Add(fileInfo.FullName, fileInfo.Name);

                    Log.Info($"Successfully Added  File: {fileInfo.Name} to Package");

                }
                Log.Info("Updating Package entries this may take time dependent on package size.");
                zipFile.CommitUpdate();
                zipFile.Close();

                Log.Info("Packaging Successfully completed.");
                return true;

            }
            catch (Exception createGoEx)
            {
                Log.Error($"Failed to Packaging Legacy Go deliverable: {sourceArchive} - {createGoEx.Message}");

                if (createGoEx.InnerException != null)
                    Log.Error($"Inner Exception: {createGoEx.InnerException.Message}");

                return false;
            }
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


        #region IDisposable

        // Flag: Has Dispose already been called?
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }

        ~ZipHandler()
        {
            Dispose(false);
        }

        #endregion
    }
}