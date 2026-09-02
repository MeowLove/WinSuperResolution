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
        internal const int RecommendedDriverAgeDays = 548;

        internal EnvironmentCompatibilitySnapshot Inspect(DisplayConfigurationRecord record, VirtualDesktopModeService virtualDesktopModes)
        {
            EnvironmentCompatibilitySnapshot snapshot = ReadPlatform(record == null ? null : record.LiveDisplay);
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

        private static EnvironmentCompatibilitySnapshot ReadPlatform(LiveDisplayInfo liveDisplay)
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

            snapshot.DetectedGraphicsSummary = JoinAdapters(adapters);
            if (liveDisplay == null)
            {
                snapshot.GraphicsSummary = snapshot.DetectedGraphicsSummary;
                snapshot.OtherGraphicsSummary = snapshot.DetectedGraphicsSummary;
                return snapshot;
            }

            snapshot.SelectedDisplaySummary = DescribeDisplay(liveDisplay);
            GraphicsAdapterInfo activeAdapter = FindActiveAdapter(adapters, liveDisplay.AdapterName);
            if (activeAdapter == null)
            {
                snapshot.GraphicsSummary = "Driver information for the active display adapter is unavailable";
                snapshot.OtherGraphicsSummary = snapshot.DetectedGraphicsSummary;
                snapshot.HasOldOrUnknownDriver = true;
                return snapshot;
            }

            snapshot.ActiveAdapterMatched = true;
            snapshot.GraphicsSummary = DescribeAdapter(activeAdapter);
            snapshot.OtherGraphicsSummary = JoinAdapters(adapters, activeAdapter);
            snapshot.DriverDate = activeAdapter.DriverDate;
            snapshot.HasOldOrUnknownDriver = IsOldOrUnknown(activeAdapter);
            return snapshot;
        }

        private static GraphicsAdapterInfo FindActiveAdapter(IEnumerable<GraphicsAdapterInfo> adapters, string adapterName)
        {
            if (string.IsNullOrEmpty(adapterName))
                return null;
            foreach (GraphicsAdapterInfo adapter in adapters)
            {
                if (string.Equals(adapter.Name, adapterName, StringComparison.OrdinalIgnoreCase))
                    return adapter;
            }
            return null;
        }

        private static string DescribeDisplay(LiveDisplayInfo display)
        {
            string name = string.IsNullOrEmpty(display.FriendlyName) ? "Display" : display.FriendlyName;
            string device = string.IsNullOrEmpty(display.DeviceName) ? "" : " [" + display.DeviceName + "]";
            string connection = string.IsNullOrEmpty(display.ConnectionTechnology) ? "" : " | " + display.ConnectionTechnology;
            return name + device + connection;
        }

        private static string JoinAdapters(IEnumerable<GraphicsAdapterInfo> adapters, GraphicsAdapterInfo excluded = null)
        {
            StringBuilder graphics = new StringBuilder();
            foreach (GraphicsAdapterInfo adapter in adapters)
            {
                if (ReferenceEquals(adapter, excluded))
                    continue;
                if (graphics.Length > 0)
                    graphics.AppendLine();
                graphics.Append(DescribeAdapter(adapter));
            }
            return graphics.ToString();
        }

        private static string DescribeAdapter(GraphicsAdapterInfo adapter)
        {
            StringBuilder text = new StringBuilder();
            text.Append(adapter.Name).Append(" | ").Append(adapter.DriverVersion);
            if (adapter.DriverDate.HasValue)
                text.Append(" | ").Append(adapter.DriverDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return text.ToString();
        }

        private static bool IsOldOrUnknown(GraphicsAdapterInfo adapter)
        {
            return adapter == null || !adapter.DriverDate.HasValue || (DateTime.Today - adapter.DriverDate.Value.Date).TotalDays > RecommendedDriverAgeDays;
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
        internal string DetectedGraphicsSummary { get; set; }
        internal string OtherGraphicsSummary { get; set; }
        internal string SelectedDisplaySummary { get; set; }
        internal string PathSummary { get; set; }
        internal DateTime? DriverDate { get; set; }
        internal bool ActiveAdapterMatched { get; set; }
        internal bool HasOldOrUnknownDriver { get; set; }
    }
}
