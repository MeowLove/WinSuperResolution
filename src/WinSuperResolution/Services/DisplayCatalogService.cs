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

        internal DisplayCatalogService(DiagnosticsService diagnostics)
        {
            _diagnostics = diagnostics;
        }

        internal IList<DisplayConfigurationRecord> Scan()
        {
            List<DisplayConfigurationRecord> records = new List<DisplayConfigurationRecord>();
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

                    FinalizeRecord(record);
                    records.Add(record);
                }
            }

            _diagnostics.Write("Scanned " + records.Count + " registered display configuration root(s).");
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

        private static void FinalizeRecord(DisplayConfigurationRecord record)
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
            record.ValidationStatus = primary.HasActiveSignal ? ValidationStatus.Warning : ValidationStatus.Warning;
            record.ScanWarning = "Live display correlation is not implemented in this milestone.";
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
