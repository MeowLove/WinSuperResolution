using System;
using System.IO;

namespace WinSuperResolution.Services
{
    internal static class AppPaths
    {
        internal static string ExecutableDirectory
        {
            get
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                return Path.GetFullPath(path);
            }
        }

        internal static string SettingsPath
        {
            get { return Path.Combine(ExecutableDirectory, "WinSuperResolution.settings.json"); }
        }

        internal static string DataRoot
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinSuperResolution"); }
        }

        internal static string BackupsDirectory { get { return Path.Combine(DataRoot, "Backups"); } }
        internal static string JournalsDirectory { get { return Path.Combine(DataRoot, "Journals"); } }
        internal static string DisplayStateDirectory { get { return Path.Combine(DataRoot, "DisplayState"); } }
        internal static string LogsDirectory { get { return Path.Combine(DataRoot, "Logs"); } }

        internal static void EnsureWritableDataDirectories()
        {
            Directory.CreateDirectory(BackupsDirectory);
            Directory.CreateDirectory(JournalsDirectory);
            Directory.CreateDirectory(DisplayStateDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }
    }
}
