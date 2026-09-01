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

        internal static string DataRoot { get { return ExecutableDirectory; } }
        internal static string LegacyDataRoot { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinSuperResolution"); } }

        internal static string BackupsDirectory { get { return Path.Combine(DataRoot, "backup_reg"); } }
        internal static string JournalsDirectory { get { return Path.Combine(DataRoot, "backup_journal"); } }
        internal static string DisplayStateDirectory { get { return Path.Combine(DataRoot, "display_state"); } }
        internal static string LogsDirectory { get { return Path.Combine(DataRoot, "logs"); } }
        internal static string LatestCapabilityJournalPath { get { return Path.Combine(JournalsDirectory, "latest-capability-journal.json"); } }
        internal static string LatestDisplayCacheResetJournalPath { get { return Path.Combine(JournalsDirectory, "latest-display-cache-reset.json"); } }
        internal static string LatestDisplayModeSnapshotPath { get { return Path.Combine(DisplayStateDirectory, "pending-display-mode.json"); } }
        internal static string LatestScaleJournalPath { get { return Path.Combine(JournalsDirectory, "latest-scale-journal.json"); } }

        internal static string ResolveLatestCapabilityJournalPath()
        {
            string portablePath = LatestCapabilityJournalPath;
            if (File.Exists(portablePath))
                return portablePath;
            string legacyPath = Path.Combine(LegacyDataRoot, "Journals", "latest-capability-journal.json");
            return File.Exists(legacyPath) ? legacyPath : portablePath;
        }

        internal static void EnsureWritableDataDirectories()
        {
            Directory.CreateDirectory(BackupsDirectory);
            Directory.CreateDirectory(JournalsDirectory);
            Directory.CreateDirectory(DisplayStateDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }
    }
}
