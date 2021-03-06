﻿using log4net;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using static System.Security.Cryptography.MD5;

namespace SchTech.File.Manager.Concrete.FileSystem
{
    public class FileDirectoryManager
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileDirectoryManager));

        public static string ReturnAdiAsAString(string adiFullName)
        {
            //in place to workaround a utf issue on the database table.
            var xdoc = XDocument.Load(adiFullName);
            xdoc.Declaration = new XDeclaration("1.0", Encoding.Unicode.HeaderName, null);
            return xdoc.ToString();
        }

        public static string GetFileSize(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;
            var fsizeInfo = new FileInfo(fileName);

            Log.Info($"Filesize retrieved for {fileName}: {fsizeInfo.Length}");
            return fsizeInfo.Length.ToString();
        }

        /// <summary>
        ///     Function to return the file md5 sum.
        ///     Used during unpacking of the source archive, also when images have completed download
        ///     This is then added to the asset section of the adi.xml
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileHash(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;
            string strHash;
            using (Stream s = System.IO.File.OpenRead(fileName))
            {
                var fileHash = Create().ComputeHash(s);
                strHash = BitConverter.ToString(fileHash).Replace("-", "");
            }

            Log.Info($"Checksum Retrieved for {fileName}: {strHash}");
            return strHash;
        }


        /// <summary>
        ///     Function to remove an existing temp directory and files
        /// </summary>
        public static void RemoveExistingTempDirectory(string outputDirectory)
        {
            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    return;
                }


                Log.Info($"Temp Directory {outputDirectory} Exists, removing");

                foreach (var file in Directory.EnumerateFiles(outputDirectory,
                    "*.*", SearchOption.AllDirectories))
                    System.IO.File.Delete(file);

                foreach (var dir in Directory.GetDirectories(outputDirectory, "*", SearchOption.AllDirectories))
                    Directory.Delete(dir);

                for (var d = 0; d <= 5; d++)
                {
                    if (!RemoveDirectory(outputDirectory))
                    {
                        Log.Info($"Temp Directory {outputDirectory} Successfully removed.");
                        break;
                    }
                    Log.Info("Working directory failed to delete retrying.");
                    Thread.Sleep(2000);
                }
            }
            catch (Exception delex)
            {
                Log.Error($"Failed to delete temp directory: {outputDirectory} - {delex.Message}");
                if (Log.IsDebugEnabled)
                    Log.Debug($"STACK TRACE: {delex.StackTrace}");
            }
        }

        private static bool RemoveDirectory(string outputdir)
        {
            if (Directory.Exists(outputdir))
                Directory.Delete(outputdir);

            return Directory.Exists(outputdir);
        }
    }
}