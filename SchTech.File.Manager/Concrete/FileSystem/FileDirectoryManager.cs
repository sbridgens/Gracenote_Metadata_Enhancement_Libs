using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Security.Cryptography.MD5;

namespace SchTech.File.Manager.Concrete.FileSystem
{   
    public class FileDirectoryManager
    {/// <summary>
        /// Initialize Log4net
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(FileDirectoryManager));
        public static string ReturnAdiAsAString(string adiFullName)
        {
            return System.IO.File.ReadAllText(adiFullName).Replace("UTF-8","UTF-16");
        }

        public static string GetFileSize(string FileName)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                FileInfo fsizeInfo = new FileInfo(FileName);
                Log.Info($"Filesize retrieved for {FileName}: {fsizeInfo.Length}");
                return fsizeInfo.Length.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Function to return the file md5 sum.
        /// Used during unpacking of the source archive, also when images have completed download
        /// This is then added to the asset section of the adi.xml
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string GetFileHash(string FileName)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                string strHash = null;
                using (Stream s = System.IO.File.OpenRead(FileName))
                {
                    var fileHash = Create().ComputeHash(s);
                    strHash = BitConverter.ToString(fileHash).Replace("-", "");
                }

                Log.Info($"Checksum Retrieved for {FileName}: {strHash}");
                return strHash;
            }

            return string.Empty;
        }


        /// <summary>
        /// Function to remove an existing temp directory and files
        /// </summary>
        public static void RemoveExistingTempDirectory(string OutputDirectory)
        {
            Log.Info($"Temp Directory {OutputDirectory} Exists, removing");
            try
            {
                foreach (var file in Directory.EnumerateFiles(OutputDirectory, "*.*", searchOption: SearchOption.AllDirectories))
                {
                    System.IO.File.Delete(file);
                }
                Directory.Delete(OutputDirectory);
                Log.Info($"Temp Directory {OutputDirectory} Successfully removed.");
                Directory.CreateDirectory(OutputDirectory);
            }
            catch (Exception delex)
            {
                Log.Error($"Failed to delete temp directory: {OutputDirectory} - {delex.Message}");
                if (Log.IsDebugEnabled)
                    Log.Debug($"STACK TRACE: {delex.StackTrace}");
            }

        }
    }
}
