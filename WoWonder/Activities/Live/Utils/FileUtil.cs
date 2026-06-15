using Android.Content;
using Android.OS;
using Java.IO;
using System;
using WoWonder.Helpers.Utils;
using Environment = Android.OS.Environment;

namespace WoWonder.Activities.Live.Utils
{
    public static class FileUtil
    {
        private static readonly string LogFolderName = "log";
        private static readonly string LogFileName = "agora-rtc.log";

        /// <summary>
        /// Initialize the log folder
        /// </summary>
        /// <param name="context">Context to find the accessible file folder</param>
        /// <returns>the absolute path of the log file</returns>
        public static string InitializeLogFile(Context context)
        {
            try
            {
                File folder;
                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.P:
                        folder = new File(context.GetExternalFilesDir(Environment.DirectoryDocuments), LogFolderName);
                        break;
                    default:
                        {
                            string path = Methods.Path.FolderDcimMyApp + File.Separator + context.PackageName + File.Separator + LogFolderName;
                            folder = new File(path);
                            if (!folder.Exists() && !folder.Mkdir()) folder = null!;
                            break;
                        }
                }

                if (folder != null && !folder.Exists() && !folder.Mkdir())
                    return "";
                return new File(folder, LogFileName).AbsolutePath;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }
    }
}