using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class DisplayCacheResetService
    {
        private const string ConfigurationRegistryPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";
        private const string ConnectivityRegistryPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Connectivity";
        private const string ScaleFactorsRegistryPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\ScaleFactors";

        private readonly JournalService _journals;
        private readonly DiagnosticsService _diagnostics;

        internal DisplayCacheResetService(JournalService journals, DiagnosticsService diagnostics)
        {
            _journals = journals;
            _diagnostics = diagnostics;
        }

        internal OperationResult Reset()
        {
            AppPaths.EnsureWritableDataDirectories();
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            string backupDirectory = Path.Combine(AppPaths.BackupsDirectory, "display-cache-reset-" + stamp);
            string journalPath = _journals.CreateTimestampedPath(AppPaths.JournalsDirectory, "display-cache-reset");
            RegistryOperationJournal journal = CreateJournal(backupDirectory);

            try
            {
                Directory.CreateDirectory(backupDirectory);
                PrepareBackup(journal, ConfigurationRegistryPath, Path.Combine(backupDirectory, "Configuration.reg"));
                PrepareBackup(journal, ConnectivityRegistryPath, Path.Combine(backupDirectory, "Connectivity.reg"));
                PrepareBackup(journal, ScaleFactorsRegistryPath, Path.Combine(backupDirectory, "ScaleFactors.reg"));
                journal.Status = "Prepared";
                _journals.Write(journalPath, journal);
                _journals.CopyAsLatest(journalPath, AppPaths.LatestDisplayCacheResetJournalPath);

                DeleteRegistryTree(journal, ConfigurationRegistryPath);
                DeleteRegistryTree(journal, ConnectivityRegistryPath);
                DeleteRegistryTree(journal, ScaleFactorsRegistryPath);

                journal.Status = "Deleted";
                _journals.Write(journalPath, journal);
                _journals.CopyAsLatest(journalPath, AppPaths.LatestDisplayCacheResetJournalPath);
                _diagnostics.Write("Windows display cache reset succeeded; restart is required.");
                return new OperationResult
                {
                    Succeeded = true,
                    Message = "Windows display caches were backed up and cleared. Restart Windows immediately to rebuild them.",
                    BackupPath = backupDirectory,
                    JournalPath = journalPath,
                    RestartRequired = true
                };
            }
            catch (Exception exception)
            {
                bool partialDelete = HasModifiedEntries(journal);
                bool restored = !partialDelete || RestoreDeletedTrees(journal, backupDirectory);
                journal.Status = !partialDelete ? "FailedBeforeDelete" : (restored ? "FailedAndRestored" : "FailedAfterPartialDelete");
                try
                {
                    _journals.Write(journalPath, journal);
                    _journals.CopyAsLatest(journalPath, AppPaths.LatestDisplayCacheResetJournalPath);
                }
                catch
                {
                    // Preserve the original failure when journal persistence is unavailable.
                }
                _diagnostics.Write("Windows display cache reset failed: " + exception.Message + "; restored=" + restored);
                return new OperationResult
                {
                    Succeeded = false,
                    Message = !partialDelete
                        ? "Windows display caches were not cleared. No restart was requested: " + exception.Message
                        : restored
                            ? "Windows display caches were not cleared and any partial deletion was restored. No restart was requested: " + exception.Message
                            : "Windows display cache reset was only partially completed. Do not restart until the backup is reviewed: " + exception.Message,
                    BackupPath = backupDirectory,
                    JournalPath = journalPath
                };
            }
        }

        private static RegistryOperationJournal CreateJournal(string backupDirectory)
        {
            RegistryOperationJournal journal = new RegistryOperationJournal();
            journal.JournalId = Guid.NewGuid().ToString("N");
            journal.OperationType = "WindowsDisplayCacheReset";
            journal.Status = "Preparing";
            journal.CreatedUtc = DateTime.UtcNow;
            journal.BackupPath = backupDirectory;
            journal.ConfigurationKey = "GraphicsDrivers display cache roots";
            return journal;
        }

        private static void PrepareBackup(RegistryOperationJournal journal, string registryPath, string backupPath)
        {
            RegistryJournalEntry entry = new RegistryJournalEntry
            {
                RegistryPath = registryPath,
                RecoveryStatus = "BackupPending"
            };
            journal.Entries.Add(entry);

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath, false))
            {
                if (key == null)
                {
                    entry.RecoveryStatus = "MissingBeforeReset";
                    return;
                }
            }

            ExportRegistryTree(registryPath, backupPath);
            entry.RecoveryStatus = "BackedUp";
        }

        private static void DeleteRegistryTree(RegistryOperationJournal journal, string registryPath)
        {
            RegistryJournalEntry entry = FindEntry(journal, registryPath);
            if (entry != null && entry.RecoveryStatus == "MissingBeforeReset")
                return;
            Registry.LocalMachine.DeleteSubKeyTree(registryPath, false);
            if (entry != null)
            {
                entry.Modified = true;
                entry.RecoveryStatus = "Deleted";
            }
        }

        private static bool RestoreDeletedTrees(RegistryOperationJournal journal, string backupDirectory)
        {
            bool restored = true;
            for (int index = journal.Entries.Count - 1; index >= 0; index--)
            {
                RegistryJournalEntry entry = journal.Entries[index];
                if (!entry.Modified)
                    continue;

                try
                {
                    ImportRegistryTree(GetBackupPath(backupDirectory, entry.RegistryPath));
                    entry.RecoveryStatus = "RestoredAfterFailure";
                }
                catch
                {
                    entry.RecoveryStatus = "RestoreFailed";
                    restored = false;
                }
            }
            return restored;
        }

        private static bool HasModifiedEntries(RegistryOperationJournal journal)
        {
            foreach (RegistryJournalEntry entry in journal.Entries)
            {
                if (entry.Modified)
                    return true;
            }
            return false;
        }

        private static RegistryJournalEntry FindEntry(RegistryOperationJournal journal, string registryPath)
        {
            foreach (RegistryJournalEntry entry in journal.Entries)
            {
                if (string.Equals(entry.RegistryPath, registryPath, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return null;
        }

        private static void ExportRegistryTree(string registryPath, string backupPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            startInfo.Arguments = "export \"HKLM\\" + registryPath + "\" \"" + backupPath + "\" /y";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0 || !File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
                    throw new InvalidOperationException("reg export did not produce a valid backup for HKLM\\" + registryPath + ". Exit code: " + process.ExitCode + ".");
            }
        }

        private static void ImportRegistryTree(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Registry backup file is missing.", backupPath);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            startInfo.Arguments = "import \"" + backupPath + "\"";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new InvalidOperationException("reg import failed for " + backupPath + ". Exit code: " + process.ExitCode + ".");
            }
        }

        private static string GetBackupPath(string backupDirectory, string registryPath)
        {
            string fileName;
            if (string.Equals(registryPath, ConfigurationRegistryPath, StringComparison.OrdinalIgnoreCase))
                fileName = "Configuration.reg";
            else if (string.Equals(registryPath, ConnectivityRegistryPath, StringComparison.OrdinalIgnoreCase))
                fileName = "Connectivity.reg";
            else
                fileName = "ScaleFactors.reg";
            return Path.Combine(backupDirectory, fileName);
        }
    }
}
