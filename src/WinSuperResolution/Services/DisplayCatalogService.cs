using System;
using System.Collections.Generic;
using Microsoft.Win32;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class DisplayCatalogService
    {
        private const string ConfigurationRootPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";
        private readonly DiagnosticsService _diagnostics;
        private readonly LiveDisplayService _liveDisplays;

        internal DisplayCatalogService(DiagnosticsService diagnostics)
        {
            _diagnostics = diagnostics;
            _liveDisplays = new LiveDisplayService();
        }

        internal IList<DisplayConfigurationRecord> Scan()
        {
            List<DisplayConfigurationRecord> records = new List<DisplayConfigurationRecord>();
            IList<LiveDisplayInfo> liveDisplays = _liveDisplays.Enumerate();
            using (RegistryKey root = Registry.LocalMachine.OpenSubKey(ConfigurationRootPath, false))
            {
                if (root == null)
                {
                    throw new InvalidOperationException("Unable to read GraphicsDrivers Configuration registry root.");
                }

                foreach (string configurationKey in root.GetSubKeyNames())
                {
                    DisplayConfigurationRecord record = new DisplayConfigurationRecord();
                    record.ConfigurationKey = configurationKey;
                    using (RegistryKey configurationRoot = root.OpenSubKey(configurationKey, false))
                    {
                        DiscoverTargets(configurationRoot, configurationKey, string.Empty, record);
                    }

                    FinalizeRecord(record, liveDisplays);
                    records.Add(record);
                }
            }

            _diagnostics.Write("Scanned " + records.Count + " registered display configuration root(s) and " + liveDisplays.Count + " active Windows display(s).");
            return records;
        }

        private static void DiscoverTargets(RegistryKey key, string configurationKey, string relativePath, DisplayConfigurationRecord record)
        {
            if (key == null)
            {
                return;
            }

            object widthValue = key.GetValue("PrimSurfSize.cx", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            object heightValue = key.GetValue("PrimSurfSize.cy", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            int width;
            int height;

            if (TryReadPositiveDword(widthValue, out width) && TryReadPositiveDword(heightValue, out height))
            {
                int activeWidth;
                int activeHeight;
                TryReadPositiveDword(key.GetValue("ActiveSize.cx", null, RegistryValueOptions.DoNotExpandEnvironmentNames), out activeWidth);
                TryReadPositiveDword(key.GetValue("ActiveSize.cy", null, RegistryValueOptions.DoNotExpandEnvironmentNames), out activeHeight);

                RegistryTarget target = new RegistryTarget();
                target.RelativePath = string.IsNullOrEmpty(relativePath) ? configurationKey : configurationKey + "\\" + relativePath;
                target.PrimarySurfaceWidth = width;
                target.PrimarySurfaceHeight = height;
                target.ActiveSignalWidth = activeWidth;
                target.ActiveSignalHeight = activeHeight;
                record.RegistryTargets.Add(target);
            }

            foreach (string childName in key.GetSubKeyNames())
            {
                string childPath = string.IsNullOrEmpty(relativePath) ? childName : relativePath + "\\" + childName;
                using (RegistryKey child = key.OpenSubKey(childName, false))
                {
                    DiscoverTargets(child, configurationKey, childPath, record);
                }
            }
        }

        private static void FinalizeRecord(DisplayConfigurationRecord record, IList<LiveDisplayInfo> liveDisplays)
        {
            if (record.RegistryTargets.Count == 0)
            {
                record.ValidationStatus = ValidationStatus.Blocked;
                record.ScanWarning = "No writable PrimSurfSize target was found.";
                return;
            }

            RegistryTarget primary = record.RegistryTargets[0];
            record.PrimarySurfaceWidth = primary.PrimarySurfaceWidth;
            record.PrimarySurfaceHeight = primary.PrimarySurfaceHeight;
            record.ActiveSignalWidth = primary.ActiveSignalWidth;
            record.ActiveSignalHeight = primary.ActiveSignalHeight;
            record.CalculationBasis = primary.HasActiveSignal ? CalculationBasis.ActiveSize : CalculationBasis.PrimSurfSize;
            record.ValidationStatus = primary.HasActiveSignal ? ValidationStatus.Ready : ValidationStatus.Warning;
            CorrelateLiveDisplay(record, liveDisplays);
        }

        private static void CorrelateLiveDisplay(DisplayConfigurationRecord record, IList<LiveDisplayInfo> liveDisplays)
        {
            List<LiveDisplayInfo> candidates = new List<LiveDisplayInfo>();
            foreach (LiveDisplayInfo display in liveDisplays)
            {
                if (MatchesResolutionEvidence(record, display))
                    candidates.Add(display);
            }

            if (candidates.Count == 0)
            {
                record.ConnectionStatus = ConnectionStatus.Historical;
                record.MatchStatus = MatchStatus.Unmatched;
                record.CorrelationEvidence = "No active Windows display has compatible resolution evidence.";
                record.ScanWarning = "Historical or uncorrelated registry configuration. It remains eligible for virtual-capability planning, but current mode and scale controls are disabled.";
                return;
            }

            if (candidates.Count > 1)
            {
                record.ConnectionStatus = ConnectionStatus.Inactive;
                record.MatchStatus = MatchStatus.Ambiguous;
                record.CorrelationEvidence = "Multiple active displays match only resolution evidence.";
                record.ScanWarning = "Ambiguous live association. The record is not treated as the current display.";
                return;
            }

            LiveDisplayInfo candidate = candidates[0];
            record.LiveDisplay = candidate;
            record.ConnectionStatus = ConnectionStatus.Active;
            if (HasStableIdentityEvidence(record.ConfigurationKey, candidate))
            {
                record.MatchStatus = MatchStatus.Exact;
                record.CorrelationEvidence = "Unique EDID/monitor identity evidence and current-mode evidence matched.";
                record.ScanWarning = string.Empty;
            }
            else
            {
                record.MatchStatus = MatchStatus.Candidate;
                record.CorrelationEvidence = "Current-mode resolution matches, but the registry key lacks a unique monitor instance token.";
                record.ScanWarning = "Candidate live association only. Current mode and experimental scaling controls stay disabled until an Exact match is proven.";
            }
        }

        private static bool MatchesResolutionEvidence(DisplayConfigurationRecord record, LiveDisplayInfo display)
        {
            if (!display.IsAttachedToDesktop || display.CurrentWidth <= 0 || display.CurrentHeight <= 0)
                return false;
            if (record.ActiveSignalWidth == display.CurrentWidth && record.ActiveSignalHeight == display.CurrentHeight)
                return true;
            return record.PrimarySurfaceWidth == display.CurrentWidth && record.PrimarySurfaceHeight == display.CurrentHeight;
        }

        private static bool HasStableIdentityEvidence(string configurationKey, LiveDisplayInfo display)
        {
            if (string.IsNullOrEmpty(configurationKey) || display == null)
                return false;
            string key = Normalize(configurationKey);
            string monitorId = Normalize(display.MonitorDeviceId);
            string devicePath = Normalize(display.MonitorDevicePath);
            if (monitorId.Length >= 12 && key.IndexOf(monitorId, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            string[] monitorIdParts = (display.MonitorDeviceId ?? string.Empty).Split('\\');
            if (monitorIdParts.Length >= 3)
            {
                string instanceToken = Normalize(monitorIdParts[2]);
                if (instanceToken.Length >= 12 && key.IndexOf(instanceToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            if (devicePath.Length >= 16 && key.IndexOf(devicePath, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            string manufacturer = Normalize(display.EdidManufacturer);
            string productCode = display.EdidProductCode > 0 ? display.EdidProductCode.ToString() : string.Empty;
            return manufacturer.Length == 3 && productCode.Length > 0 && key.IndexOf(manufacturer + productCode, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            char[] buffer = new char[value.Length];
            int count = 0;
            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character))
                    buffer[count++] = char.ToUpperInvariant(character);
            }
            return new string(buffer, 0, count);
        }

        private static bool TryReadPositiveDword(object value, out int result)
        {
            result = 0;
            if (value is int)
            {
                result = (int)value;
                return result > 0;
            }

            if (value is uint && (uint)value <= int.MaxValue)
            {
                result = (int)(uint)value;
                return result > 0;
            }

            return false;
        }
    }
}
