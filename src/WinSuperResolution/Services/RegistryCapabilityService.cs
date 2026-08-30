using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class RegistryCapabilityService
    {
        private const string ConfigurationRootPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";
        private readonly JournalService _journals;
        private readonly DiagnosticsService _diagnostics;

        internal RegistryCapabilityService(JournalService journals, DiagnosticsService diagnostics)
        {
            _journals = journals;
            _diagnostics = diagnostics;
        }

        internal OperationResult Apply(ResolutionPlan plan)
        {
            if (plan == null || plan.Record == null || plan.Mutations.Count == 0)
                return Failure("A complete virtual-resolution capability plan is required.");
            return ApplyBatch(new List<ResolutionPlan> { plan });
        }

        internal OperationResult ApplyBatch(IList<ResolutionPlan> plans)
        {
            if (plans == null || plans.Count == 0)
                return Failure("At least one virtual-resolution capability plan is required.");

            AppPaths.EnsureWritableDataDirectories();
            try
            {
                foreach (ResolutionPlan plan in plans)
                {
                    if (plan == null || plan.Record == null || plan.Mutations.Count == 0)
                        throw new InvalidOperationException("The batch contains an incomplete capability plan.");
                    Preflight(plan);
                }
                string backupPath = ExportConfigurationBackup();
                RegistryOperationJournal journal = CreateJournal(plans, backupPath);
                string journalPath = _journals.CreateTimestampedPath(AppPaths.JournalsDirectory, "capability-journal");
                _journals.Write(journalPath, journal);
                _journals.CopyAsLatest(journalPath, AppPaths.LatestCapabilityJournalPath);

                try
                {
                    foreach (RegistryJournalEntry entry in journal.Entries)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(entry.RegistryPath, true))
                        {
                            if (key == null)
                                throw new InvalidOperationException("Writable registry key disappeared: HKLM\\" + entry.RegistryPath);
                            entry.Modified = true;
                            key.SetValue(entry.WidthValueName, entry.TargetWidth, RegistryValueKind.DWord);
                            key.SetValue(entry.HeightValueName, entry.TargetHeight, RegistryValueKind.DWord);
                            VerifyEntry(key, entry, true);
                        }
                    }

                    journal.Status = "Applied";
                    _journals.Write(journalPath, journal);
                    _journals.CopyAsLatest(journalPath, AppPaths.LatestCapabilityJournalPath);
                    _diagnostics.Write("Virtual-resolution capability apply succeeded: " + journal.JournalId);
                    return new OperationResult
                    {
                        Succeeded = true,
                        Message = "Virtual-resolution capability batch was written and verified. Reboot or restart the display driver before expecting new current modes.",
                        JournalPath = journalPath,
                        BackupPath = backupPath,
                        RestartRequired = true
                    };
                }
                catch (Exception exception)
                {
                    journal.Status = "ApplyingFailed";
                    RecoverEntries(journal);
                    _journals.Write(journalPath, journal);
                    _journals.CopyAsLatest(journalPath, AppPaths.LatestCapabilityJournalPath);
                    _diagnostics.Write("Virtual-resolution capability apply failed: " + exception.Message);
                    return new OperationResult
                    {
                        Succeeded = false,
                        Message = "Apply failed and recovery was attempted: " + exception.Message,
                        JournalPath = journalPath,
                        BackupPath = backupPath
                    };
                }
            }
            catch (Exception exception)
            {
                _diagnostics.Write("Virtual-resolution capability preflight failed: " + exception.Message);
                return Failure(exception.Message);
            }
        }

        internal OperationResult RestoreLatest()
        {
            string journalPath = AppPaths.ResolveLatestCapabilityJournalPath();
            if (!File.Exists(journalPath))
                return Failure("No virtual-resolution capability journal is available to restore.");

            try
            {
                RegistryOperationJournal journal = _journals.Read<RegistryOperationJournal>(journalPath);
                RecoverEntries(journal);
                journal.Status = HasResidualFailure(journal) ? "ResidualFailure" : "Recovered";
                _journals.Write(journalPath, journal);
                return new OperationResult
                {
                    Succeeded = !HasResidualFailure(journal),
                    Message = journal.Status == "Recovered" ? "The latest capability journal was restored and verified." : "Recovery completed with residual failures; inspect the journal for exact paths.",
                    JournalPath = journalPath,
                    BackupPath = journal.BackupPath
                };
            }
            catch (Exception exception)
            {
                return Failure("Recovery could not start: " + exception.Message);
            }
        }

        private void Preflight(ResolutionPlan plan)
        {
            if (plan.TargetWidth <= 0 || plan.TargetHeight <= 0)
                throw new InvalidOperationException("The target resolution is invalid.");
            if (plan.TargetWidth > int.MaxValue || plan.TargetHeight > int.MaxValue)
                throw new InvalidOperationException("The target resolution exceeds the DWORD range.");

            foreach (RegistryMutation mutation in plan.Mutations)
            {
                string keyPath = ConfigurationRootPath + "\\" + mutation.RelativePath;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key == null)
                        throw new InvalidOperationException("Registry target is not writable: HKLM\\" + keyPath);
                    int currentWidth = ReadDword(key, "PrimSurfSize.cx");
                    int currentHeight = ReadDword(key, "PrimSurfSize.cy");
                    if (currentWidth != mutation.OriginalWidth || currentHeight != mutation.OriginalHeight)
                        throw new InvalidOperationException("Registry target changed since the plan was built: HKLM\\" + keyPath);
                }
            }
        }

        private string ExportConfigurationBackup()
        {
            string backupPath = Path.Combine(AppPaths.BackupsDirectory, "GraphicsDrivers-Configuration-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".reg");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            startInfo.Arguments = "export \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\" \"" + backupPath + "\" /y";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0 || !File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
                    throw new InvalidOperationException("reg export did not produce a valid backup. Exit code: " + process.ExitCode + ".");
            }
            return backupPath;
        }

        private static RegistryOperationJournal CreateJournal(IList<ResolutionPlan> plans, string backupPath)
        {
            RegistryOperationJournal journal = new RegistryOperationJournal();
            journal.JournalId = Guid.NewGuid().ToString("N");
            journal.OperationType = "VirtualResolutionCapability";
            journal.Status = "Prepared";
            journal.CreatedUtc = DateTime.UtcNow;
            journal.BackupPath = backupPath;
            journal.ConfigurationKey = plans.Count == 1 ? plans[0].Record.ConfigurationKey : "Batch(" + plans.Count + ")";
            foreach (ResolutionPlan plan in plans)
            {
                foreach (RegistryMutation mutation in plan.Mutations)
                {
                    journal.Entries.Add(new RegistryJournalEntry
                    {
                        RegistryPath = ConfigurationRootPath + "\\" + mutation.RelativePath,
                        WidthValueName = "PrimSurfSize.cx",
                        HeightValueName = "PrimSurfSize.cy",
                        OriginalWidth = mutation.OriginalWidth,
                        OriginalHeight = mutation.OriginalHeight,
                        TargetWidth = mutation.TargetWidth,
                        TargetHeight = mutation.TargetHeight,
                        ValueKind = RegistryValueKind.DWord.ToString(),
                        OriginalWidthBytes = Convert.ToBase64String(BitConverter.GetBytes(mutation.OriginalWidth)),
                        OriginalHeightBytes = Convert.ToBase64String(BitConverter.GetBytes(mutation.OriginalHeight)),
                        TargetWidthBytes = Convert.ToBase64String(BitConverter.GetBytes(mutation.TargetWidth)),
                        TargetHeightBytes = Convert.ToBase64String(BitConverter.GetBytes(mutation.TargetHeight)),
                        RecoveryStatus = "NotModified"
                    });
                }
            }
            return journal;
        }

        private static int ReadDword(RegistryKey key, string valueName)
        {
            object value = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value is int && (int)value > 0)
                return (int)value;
            if (value is uint && (uint)value <= int.MaxValue && (uint)value > 0)
                return (int)(uint)value;
            throw new InvalidOperationException("Expected positive DWORD value is missing: " + valueName);
        }

        private static void VerifyEntry(RegistryKey key, RegistryJournalEntry entry, bool target)
        {
            int expectedWidth = target ? entry.TargetWidth : entry.OriginalWidth;
            int expectedHeight = target ? entry.TargetHeight : entry.OriginalHeight;
            if (ReadDword(key, entry.WidthValueName) != expectedWidth || ReadDword(key, entry.HeightValueName) != expectedHeight)
                throw new InvalidOperationException("Registry verification failed: HKLM\\" + entry.RegistryPath);
        }

        private static bool HasResidualFailure(RegistryOperationJournal journal)
        {
            foreach (RegistryJournalEntry entry in journal.Entries)
            {
                if (entry.RecoveryStatus == "ResidualFailure")
                    return true;
            }
            return false;
        }

        private static void RecoverEntries(RegistryOperationJournal journal)
        {
            for (int index = journal.Entries.Count - 1; index >= 0; index--)
            {
                RegistryJournalEntry entry = journal.Entries[index];
                if (!entry.Modified)
                {
                    entry.RecoveryStatus = "NotModified";
                    continue;
                }
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(entry.RegistryPath, true))
                    {
                        if (key == null)
                            throw new InvalidOperationException("Registry key is no longer writable.");
                        key.SetValue(entry.WidthValueName, entry.OriginalWidth, RegistryValueKind.DWord);
                        key.SetValue(entry.HeightValueName, entry.OriginalHeight, RegistryValueKind.DWord);
                        VerifyEntry(key, entry, false);
                        entry.RecoveryStatus = "Recovered";
                    }
                }
                catch
                {
                    entry.RecoveryStatus = "ResidualFailure";
                }
            }
        }

        private static OperationResult Failure(string message)
        {
            return new OperationResult { Succeeded = false, Message = message };
        }
    }
}
