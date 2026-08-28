using System;
using System.Collections.Generic;
using System.Security.Principal;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    // There is no documented Windows API for writing a per-monitor scale. Profiles are empty
    // until a backend has been independently validated for one exact OS/display mapping.
    internal sealed class ExperimentalScaleService
    {
        private readonly JournalService _journals;
        private readonly IList<ScaleCompatibilityProfile> _profiles;

        internal ExperimentalScaleService(JournalService journals)
        {
            _journals = journals;
            _profiles = new List<ScaleCompatibilityProfile>();
        }

        internal IList<int> GetAvailableScalePercentages(DisplayConfigurationRecord record)
        {
            ScaleCompatibilityProfile profile = FindProfile(record);
            return profile == null || profile.AllowedScalePercentages == null
                ? new List<int>()
                : new List<int>(profile.AllowedScalePercentages);
        }

        internal string GetAvailability(DisplayConfigurationRecord record)
        {
            if (record == null)
                return "Experimental scaling: select a display record first.";
            if (!record.CanManageCurrentState)
                return "Experimental scaling is disabled: it requires an Active + Exact live display match.";
            if (record.LiveDisplay.CurrentScalePercent <= 0)
                return "Experimental scaling is disabled: current per-monitor scale could not be read.";
            if (FindProfile(record) == null)
                return "Experimental scaling is disabled: no verified compatibility profile matches this Windows build, user session, and display mapping. Open Windows Display Settings to change scale manually.";
            return "Experimental scaling is available for this verified profile.";
        }

        internal OperationResult Apply(DisplayConfigurationRecord record, int targetScalePercent)
        {
            ScaleCompatibilityProfile profile = FindProfile(record);
            if (profile == null)
                return new OperationResult { Succeeded = false, Message = GetAvailability(record) };
            if (!profile.AllowedScalePercentages.Contains(targetScalePercent))
                return new OperationResult { Succeeded = false, Message = "The requested scale is not allowed by the verified compatibility profile." };

            // This is intentionally unreachable in the current release because no profile exists.
            // A future backend must create the complete journal before it writes a byte.
            ScaleJournal journal = CreateJournal(record, targetScalePercent);
            string journalPath = _journals.CreateTimestampedPath(AppPaths.JournalsDirectory, "scale-journal");
            _journals.Write(journalPath, journal);
            _journals.CopyAsLatest(journalPath, AppPaths.LatestScaleJournalPath);
            return new OperationResult
            {
                Succeeded = false,
                JournalPath = journalPath,
                Message = "The scale journal was prepared, but no approved experimental scale writer is embedded in this build. No settings were changed."
            };
        }

        private ScaleCompatibilityProfile FindProfile(DisplayConfigurationRecord record)
        {
            if (record == null || !record.CanManageCurrentState || record.LiveDisplay == null)
                return null;
            Version windowsVersion = Environment.OSVersion.Version;
            string monitorId = record.LiveDisplay.MonitorDeviceId ?? string.Empty;
            foreach (ScaleCompatibilityProfile profile in _profiles)
            {
                if (!profile.IsVerified || profile.AllowedScalePercentages == null || profile.AllowedScalePercentages.Count == 0)
                    continue;
                if (profile.MinimumWindowsVersion != null && windowsVersion < profile.MinimumWindowsVersion)
                    continue;
                if (profile.MaximumWindowsVersion != null && windowsVersion > profile.MaximumWindowsVersion)
                    continue;
                if (!string.Equals(profile.Architecture, Environment.Is64BitOperatingSystem ? "x64" : "x86", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.IsNullOrEmpty(profile.MonitorDeviceIdPrefix) || !monitorId.StartsWith(profile.MonitorDeviceIdPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                return profile;
            }
            return null;
        }

        private static ScaleJournal CreateJournal(DisplayConfigurationRecord record, int targetScalePercent)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return new ScaleJournal
            {
                JournalId = Guid.NewGuid().ToString("N"),
                Status = "PreparedNoWriter",
                CreatedUtc = DateTime.UtcNow,
                UserIdentity = identity == null ? string.Empty : identity.Name,
                WindowsVersion = Environment.OSVersion.VersionString,
                DisplayIdentityEvidence = record.CorrelationEvidence + " | " + record.LiveDisplay.MonitorDeviceId,
                OriginalScalePercent = record.LiveDisplay.CurrentScalePercent,
                TargetScalePercent = targetScalePercent
            };
        }
    }
}
