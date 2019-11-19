using System;
using System.IO;
using log4net;

namespace SchTech.File.Manager.Concrete.FileSystem
{
    public sealed class HardwareInformationManager : IDisposable
    {
        private const double BytesInMb = 1048576;
        private const double BytesInGb = 1073741824;

        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(HardwareInformationManager));

        // Flag: Has Dispose already been called?
        private bool _disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        private void Dispose(bool disposing)
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

        ~HardwareInformationManager()
        {
            Dispose(false);
        }

        public bool GetDriveSpace()
        {
            try
            {
                var driveInfos = DriveInfo.GetDrives();

                foreach (var drive in driveInfos)
                {
                    // Skip to next loop cycle when drive is not ready
                    if (!drive.IsReady)
                        continue;

                    var usedSpace = Convert.ToInt32((drive.TotalSize - drive.TotalFreeSpace) / BytesInGb);
                    var freespace = Convert.ToInt32(drive.TotalFreeSpace / BytesInGb);
                    var totalsize = Convert.ToInt32(drive.TotalSize / BytesInGb);

                    log.Info($"\nDrive: {drive.Name} ({drive.DriveType}, {drive.DriveFormat})\n" +
                             $"  Used space:\t{(drive.TotalSize - drive.TotalFreeSpace) / BytesInMb} " +
                             $"MB\t{usedSpace} GB\n" +
                             $"  Free space:\t{drive.TotalFreeSpace / BytesInMb} MB\t{freespace} GB\n" +
                             $"  Total size:\t{drive.TotalSize / BytesInMb} MB\t{totalsize} GB\n\n");


                    if (drive.Name.ToLower() == "d:\\" && freespace < 50)
                        throw new Exception(
                            $"Drive Space on {drive.VolumeLabel} is less that 50GB, this service will stop running!");
                }

                return true;
            }
            catch (Exception gdsEx)
            {
                log.Error($"[GetDriveSpace]\t{gdsEx.Message}");
                return false;
            }
        }
    }
}