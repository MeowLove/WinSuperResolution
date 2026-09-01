using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    // Per-monitor scale storage is an experimental Windows compatibility route.
    // It is enabled only when one current-user PerMonitorSettings target can be resolved safely.
    internal sealed class ExperimentalScaleService
    {
        private const string PerMonitorSettingsPath = @"Control Panel\Desktop\PerMonitorSettings";
        private const string DpiValueName = "DpiValue";
        private readonly JournalService _journals;
        private readonly DiagnosticsService _diagnostics;

        internal ExperimentalScaleService(JournalService journals, DiagnosticsService diagnostics)
        {
            if (journals == null)
                throw new ArgumentNullException("journals");
            _journals = journals;
            _diagnostics = diagnostics ?? new DiagnosticsService();
        }

        internal IList<int> GetAvailableScalePercentages(DisplayConfigurationRecord record)
        {
            ScaleRegistryTarget target = FindTarget(record);
            List<int> values = new List<int>();
            if (target == null || record == null || record.LiveDisplay == null || record.LiveDisplay.CurrentScalePercent <= 0)
                return values;

            for (int value = 100; value <= 500; value += 25)
                values.Add(value);
            return values;
        }

        internal ScaleAvailabilityStatus GetAvailabilityStatus(DisplayConfigurationRecord record)
        {
            if (record == null)
                return ScaleAvailabilityStatus.NoSelection;
            if (record.ConnectionStatus != ConnectionStatus.Active || record.LiveDisplay == null)
                return ScaleAvailabilityStatus.RequiresActiveDisplay;
            if (record.LiveDisplay.CurrentScalePercent <= 0)
                return ScaleAvailabilityStatus.CurrentScaleUnavailable;
            return FindTarget(record) == null
                ? ScaleAvailabilityStatus.NoCompatibleSettingsTarget
                : ScaleAvailabilityStatus.Available;
        }

        internal OperationResult Apply(DisplayConfigurationRecord record, int targetScalePercent)
        {
            ScaleRegistryTarget target = FindTarget(record);
            if (target == null)
                return Failure("No compatible current-user per-monitor scale registry target was found.");
            if (record.LiveDisplay.CurrentScalePercent <= 0 || targetScalePercent < 100 || targetScalePercent > 500 || (targetScalePercent - record.LiveDisplay.CurrentScalePercent) % 25 != 0)
                return Failure("The requested scale is not compatible with the detected 25-percent experimental scale steps.");

            string backupPath;
            ScaleJournal journal;
            string journalPath;
            int targetDpiValue;
            try
            {
                int baselineScalePercent = GetBaselineScalePercent(record.LiveDisplay.CurrentScalePercent, target.CurrentDpiValue);
                targetDpiValue = CalculateTargetDpiValue(baselineScalePercent, targetScalePercent);
                _diagnostics.Write("Experimental scale preflight: key=" + target.RegistryPath + ", currentScale=" + record.LiveDisplay.CurrentScalePercent + ", currentDpiValue=" + target.CurrentDpiValue + ", baselineScale=" + baselineScalePercent + ", targetScale=" + targetScalePercent + ", targetDpiValue=" + targetDpiValue);
                AppPaths.EnsureWritableDataDirectories();
                backupPath = ExportScaleBackup(target);
                journal = CreateJournal(record, target, targetScalePercent, baselineScalePercent, targetDpiValue);
                journalPath = _journals.CreateTimestampedPath(AppPaths.JournalsDirectory, "scale-journal");
                journal.Entries[0].TargetBytes = Convert.ToBase64String(BitConverter.GetBytes(targetDpiValue));
                _journals.Write(journalPath, journal);
                _journals.CopyAsLatest(journalPath, AppPaths.LatestScaleJournalPath);
                _diagnostics.Write("Experimental scale journal prepared: " + journalPath + "; backup=" + backupPath);
            }
            catch (Exception exception)
            {
                return Failure("Experimental scale preflight failed: " + exception.Message);
            }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(target.RegistryPath, true))
                {
                    if (key == null)
                        throw new InvalidOperationException("The per-monitor scale registry target is no longer writable.");
                    WriteDpiValue(key, targetDpiValue);
                    int actualDpiValue;
                    if (!TryReadDpiValue(key.GetValue(DpiValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames), out actualDpiValue) || actualDpiValue != targetDpiValue)
                        throw new InvalidOperationException("The per-monitor scale registry value could not be verified.");
                }

                journal.Status = "AppliedAwaitingRestart";
                _journals.Write(journalPath, journal);
                _journals.CopyAsLatest(journalPath, AppPaths.LatestScaleJournalPath);
                _diagnostics.Write("Experimental scale write verified: " + journalPath);
                return new OperationResult
                {
                    Succeeded = true,
                    Message = "Experimental per-monitor scale was written and verified. Sign out or restart Windows before expecting it to take effect.",
                    JournalPath = journalPath,
                    BackupPath = backupPath,
                    RestartRequired = true
                };
            }
            catch (Exception exception)
            {
                Exception restoreException;
                bool restored = TryRestoreJournal(journal, out restoreException);
                journal.Status = restored ? "ApplyFailedRestored" : "ApplyFailedResidualFailure";
                try
                {
                    _journals.Write(journalPath, journal);
                    _journals.CopyAsLatest(journalPath, AppPaths.LatestScaleJournalPath);
                }
                catch (Exception journalException)
                {
                    _diagnostics.Write("Experimental scale failure journal could not be updated: " + journalException);
                }
                _diagnostics.Write("Experimental scale write failed: " + exception + "; restored=" + restored + (restoreException == null ? string.Empty : "; restoreError=" + restoreException));
                return new OperationResult
                {
                    Succeeded = false,
                    Message = restored
                        ? "Experimental scale write failed and the prior value was restored: " + exception.Message
                        : "Experimental scale write failed and automatic restoration also failed. Check the portable backup and journal: " + exception.Message,
                    JournalPath = journalPath,
                    BackupPath = backupPath
                };
            }
        }

        internal OperationResult RestoreLatest()
        {
            if (!File.Exists(AppPaths.LatestScaleJournalPath))
                return Failure("No experimental scale journal is available to restore.");
            try
            {
                ScaleJournal journal = _journals.Read<ScaleJournal>(AppPaths.LatestScaleJournalPath);
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                string currentIdentity = identity == null ? string.Empty : identity.Name;
                if (!string.Equals(journal.UserIdentity, currentIdentity, StringComparison.OrdinalIgnoreCase))
                    return Failure("The scale journal belongs to a different Windows user.");
                if (!string.Equals(journal.WindowsVersion, Environment.OSVersion.VersionString, StringComparison.Ordinal))
                    return Failure("The Windows version changed since this scale journal was created; automatic restore is blocked.");
                RestoreJournal(journal);
                journal.Status = "RestoredAwaitingRestart";
                _journals.Write(AppPaths.LatestScaleJournalPath, journal);
                return new OperationResult
                {
                    Succeeded = true,
                    Message = "The latest experimental scale journal was restored. Sign out or restart Windows before expecting it to take effect.",
                    JournalPath = AppPaths.LatestScaleJournalPath,
                    RestartRequired = true
                };
            }
            catch (Exception exception)
            {
                return Failure("Experimental scale restore could not start: " + exception.Message);
            }
        }

        private ScaleRegistryTarget FindTarget(DisplayConfigurationRecord record)
        {
            if (record == null || record.ConnectionStatus != ConnectionStatus.Active || record.LiveDisplay == null)
                return null;
            List<ScaleRegistryTarget> candidates = new List<ScaleRegistryTarget>();
            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(PerMonitorSettingsPath, false))
            {
                if (root == null)
                    return null;
                foreach (string keyName in root.GetSubKeyNames())
                {
                    using (RegistryKey key = root.OpenSubKey(keyName, false))
                    {
                        if (key == null)
                            continue;
                        int dpiValue;
                        if (!TryReadDpiValue(key.GetValue(DpiValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames), out dpiValue))
                            continue;
                        ScaleRegistryTarget candidate = new ScaleRegistryTarget
                        {
                            KeyName = keyName,
                            RegistryPath = PerMonitorSettingsPath + "\\" + keyName,
                            CurrentDpiValue = dpiValue
                        };
                        if (IsDirectIdentityMatch(record, keyName))
                            return candidate;
                        candidates.Add(candidate);
                    }
                }
            }
            return candidates.Count == 1 ? candidates[0] : null;
        }

        private string ExportScaleBackup(ScaleRegistryTarget target)
        {
            string backupName = "PerMonitorSettings-" + CreateShortBackupToken(target.RegistryPath) + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".reg";
            string backupPath = Path.Combine(AppPaths.BackupsDirectory, backupName);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            startInfo.Arguments = "export \"HKCU\\" + target.RegistryPath + "\" \"" + backupPath + "\" /y";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0 || !File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
                    throw new InvalidOperationException("reg export did not produce a valid per-monitor scale backup. Exit code: " + process.ExitCode + ".");
            }
            return backupPath;
        }

        private static string CreateShortBackupToken(string registryPath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digest = sha256.ComputeHash(Encoding.UTF8.GetBytes(registryPath ?? string.Empty));
                return BitConverter.ToString(digest).Replace("-", string.Empty).Substring(0, 12).ToLowerInvariant();
            }
        }

        private static ScaleJournal CreateJournal(DisplayConfigurationRecord record, ScaleRegistryTarget target, int targetScalePercent, int baselineScalePercent, int targetDpiValue)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            ScaleJournal journal = new ScaleJournal();
            journal.JournalId = Guid.NewGuid().ToString("N");
            journal.Status = "Prepared";
            journal.CreatedUtc = DateTime.UtcNow;
            journal.UserIdentity = identity == null ? string.Empty : identity.Name;
            journal.WindowsVersion = Environment.OSVersion.VersionString;
            journal.DisplayIdentityEvidence = record.DisplayIdentity + " | " + record.LiveDisplay.MonitorDeviceId + " | " + target.KeyName;
            journal.OriginalScalePercent = record.LiveDisplay.CurrentScalePercent;
            journal.BaselineScalePercent = baselineScalePercent;
            journal.TargetScalePercent = targetScalePercent;
            journal.Entries.Add(new ScaleJournalEntry
            {
                RegistryPath = target.RegistryPath,
                ValueName = DpiValueName,
                OriginalBytes = Convert.ToBase64String(BitConverter.GetBytes(unchecked((uint)target.CurrentDpiValue))),
                TargetBytes = Convert.ToBase64String(BitConverter.GetBytes(targetDpiValue))
            });
            return journal;
        }

        private static void RestoreJournal(ScaleJournal journal)
        {
            if (journal == null || journal.Entries == null)
                throw new InvalidOperationException("The scale journal is incomplete.");
            foreach (ScaleJournalEntry entry in journal.Entries)
            {
                byte[] bytes = Convert.FromBase64String(entry.OriginalBytes);
                if (bytes.Length != 4)
                    throw new InvalidOperationException("The scale journal contains an invalid original value.");
                int originalValue = unchecked((int)BitConverter.ToUInt32(bytes, 0));
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(entry.RegistryPath, true))
                {
                    if (key == null)
                        throw new InvalidOperationException("The scale registry target is no longer writable.");
                    WriteDpiValue(key, originalValue);
                    int actualValue;
                    if (!TryReadDpiValue(key.GetValue(entry.ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames), out actualValue) || actualValue != originalValue)
                        throw new InvalidOperationException("The restored scale value could not be verified.");
                }
            }
        }

        private static bool IsDirectIdentityMatch(DisplayConfigurationRecord record, string keyName)
        {
            string normalizedKeyName = Normalize(keyName);
            if (normalizedKeyName.Length == 0)
                return false;
            string configurationKey = Normalize(record.ConfigurationKey);
            if (configurationKey.Length >= 8 && (configurationKey.IndexOf(normalizedKeyName, StringComparison.OrdinalIgnoreCase) >= 0 || normalizedKeyName.IndexOf(configurationKey, StringComparison.OrdinalIgnoreCase) >= 0))
                return true;
            string[] monitorParts = (record.LiveDisplay.MonitorDeviceId ?? string.Empty).Split('\\');
            if (monitorParts.Length < 2)
                return false;
            string monitorModel = Normalize(monitorParts[1]);
            return monitorModel.Length >= 5 && normalizedKeyName.IndexOf(monitorModel, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryReadDpiValue(object value, out int dpiValue)
        {
            if (value is int)
            {
                dpiValue = (int)value;
                return true;
            }
            if (value is uint)
            {
                dpiValue = unchecked((int)(uint)value);
                return true;
            }
            dpiValue = 0;
            return false;
        }

        internal static int GetBaselineScalePercent(int currentScalePercent, int currentDpiValue)
        {
            if (currentScalePercent < 100 || currentScalePercent > 500 || currentScalePercent % 25 != 0)
                throw new InvalidOperationException("The current Windows scale is outside the experimental 25-percent steps.");
            int baseline = checked(currentScalePercent - checked(currentDpiValue * 25));
            if (baseline < 100 || baseline > 500 || baseline % 25 != 0)
                throw new InvalidOperationException("The PerMonitorSettings DpiValue does not resolve to a supported baseline scale.");
            return baseline;
        }

        internal static int CalculateTargetDpiValue(int baselineScalePercent, int targetScalePercent)
        {
            if (baselineScalePercent < 100 || baselineScalePercent > 500 || baselineScalePercent % 25 != 0)
                throw new InvalidOperationException("The baseline scale is outside the supported experimental range.");
            if (targetScalePercent < 100 || targetScalePercent > 500 || targetScalePercent % 25 != 0)
                throw new InvalidOperationException("The target scale is outside the supported experimental range.");
            return checked((targetScalePercent - baselineScalePercent) / 25);
        }

        private static void WriteDpiValue(RegistryKey key, int dpiValue)
        {
            key.SetValue(DpiValueName, dpiValue, RegistryValueKind.DWord);
        }

        private bool TryRestoreJournal(ScaleJournal journal, out Exception failure)
        {
            try
            {
                RestoreJournal(journal);
                failure = null;
                return true;
            }
            catch (Exception exception)
            {
                failure = exception;
                return false;
            }
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            StringBuilder builder = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character))
                    builder.Append(char.ToUpperInvariant(character));
            }
            return builder.ToString();
        }

        private static OperationResult Failure(string message)
        {
            return new OperationResult { Succeeded = false, Message = message };
        }

        private sealed class ScaleRegistryTarget
        {
            internal string KeyName { get; set; }
            internal string RegistryPath { get; set; }
            internal int CurrentDpiValue { get; set; }
        }
    }
}
