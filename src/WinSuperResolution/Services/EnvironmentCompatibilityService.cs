using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Text;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class EnvironmentCompatibilityService
    {
        private const int RecommendedDriverAgeDays = 548;

        internal EnvironmentCompatibilitySnapshot Inspect(DisplayConfigurationRecord record, VirtualDesktopModeService virtualDesktopModes)
        {
            EnvironmentCompatibilitySnapshot snapshot = ReadPlatform();
            if (record == null || record.LiveDisplay == null)
            {
                snapshot.Status = EnvironmentCompatibilityStatus.Experimental;
                snapshot.PathSummary = "Select an active display to inspect its virtual desktop path.";
                snapshot.Reason = "No active display path is selected.";
                return snapshot;
            }

            DisplayMode mode;
            string error = null;
            if (virtualDesktopModes != null && virtualDesktopModes.TryGetCurrentDesktopMode(record.LiveDisplay.DeviceName, out mode, out error))
            {
                snapshot.PathSummary = "Virtual desktop path: supported for " + record.LiveDisplay.DeviceName + ".";
                if (snapshot.HasOldOrUnknownDriver)
                {
                    snapshot.Status = EnvironmentCompatibilityStatus.Experimental;
                    snapshot.Reason = "The active path supports virtual desktop modes, but the graphics driver is old or its date could not be read.";
                }
                else
                {
                    snapshot.Status = EnvironmentCompatibilityStatus.CanTry;
                    snapshot.Reason = "The active path reports virtual desktop support and the detected graphics driver is current enough for the tested range.";
                }
                return snapshot;
            }

            snapshot.Status = EnvironmentCompatibilityStatus.Unsupported;
            snapshot.PathSummary = "Virtual desktop path: not available for " + record.LiveDisplay.DeviceName + ".";
            snapshot.Reason = string.IsNullOrEmpty(error) ? "Windows could not confirm virtual desktop support for the active path." : error;
            return snapshot;
        }

        private static EnvironmentCompatibilitySnapshot ReadPlatform()
        {
            EnvironmentCompatibilitySnapshot snapshot = new EnvironmentCompatibilitySnapshot();
            snapshot.WindowsSummary = Environment.OSVersion.VersionString + " | " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
            snapshot.ProcessorSummary = ReadFirstValue("SELECT Name FROM Win32_Processor", "Name", "Processor information unavailable");

            List<GraphicsAdapterInfo> adapters = new List<GraphicsAdapterInfo>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, DriverDate FROM Win32_VideoController"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject result in results)
                    {
                        GraphicsAdapterInfo adapter = new GraphicsAdapterInfo();
                        adapter.Name = ReadValue(result, "Name", "Graphics adapter unavailable");
                        adapter.DriverVersion = ReadValue(result, "DriverVersion", "unknown");
                        adapter.DriverDate = ParseDriverDate(ReadValue(result, "DriverDate", string.Empty));
                        adapters.Add(adapter);
                    }
                }
            }
            catch
            {
                // Hardware inventory is advisory; a denied WMI provider must not affect display operations.
            }

            if (adapters.Count == 0)
            {
                snapshot.GraphicsSummary = "Graphics adapter and driver information unavailable";
                snapshot.HasOldOrUnknownDriver = true;
            }
            else
            {
                StringBuilder graphics = new StringBuilder();
                foreach (GraphicsAdapterInfo adapter in adapters)
                {
                    if (graphics.Length > 0)
                        graphics.Append("; ");
                    graphics.Append(adapter.Name).Append(" | ").Append(adapter.DriverVersion);
                    if (adapter.DriverDate.HasValue)
                        graphics.Append(" | ").Append(adapter.DriverDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    else
                        snapshot.HasOldOrUnknownDriver = true;
                    if (adapter.DriverDate.HasValue && (DateTime.UtcNow.Date - adapter.DriverDate.Value.Date).TotalDays > RecommendedDriverAgeDays)
                        snapshot.HasOldOrUnknownDriver = true;
                }
                snapshot.GraphicsSummary = graphics.ToString();
            }
            return snapshot;
        }

        private static string ReadFirstValue(string query, string propertyName, string fallback)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject result in results)
                        return ReadValue(result, propertyName, fallback);
                }
            }
            catch
            {
                // Hardware inventory is advisory; a denied WMI provider must not affect display operations.
            }
            return fallback;
        }

        private static string ReadValue(ManagementObject result, string propertyName, string fallback)
        {
            object value = result[propertyName];
            string text = value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(text) ? fallback : text;
        }

        private static DateTime? ParseDriverDate(string value)
        {
            DateTime date;
            if (string.IsNullOrEmpty(value) || value.Length < 8 || !DateTime.TryParseExact(value.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;
            return date;
        }

        private sealed class GraphicsAdapterInfo
        {
            internal string Name { get; set; }
            internal string DriverVersion { get; set; }
            internal DateTime? DriverDate { get; set; }
        }
    }

    internal enum EnvironmentCompatibilityStatus
    {
        Unsupported,
        Experimental,
        CanTry
    }

    internal sealed class EnvironmentCompatibilitySnapshot
    {
        internal EnvironmentCompatibilityStatus Status { get; set; }
        internal string Reason { get; set; }
        internal string WindowsSummary { get; set; }
        internal string ProcessorSummary { get; set; }
        internal string GraphicsSummary { get; set; }
        internal string PathSummary { get; set; }
        internal bool HasOldOrUnknownDriver { get; set; }
    }
}
