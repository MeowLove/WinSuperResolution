using System;
using System.Collections.Generic;

namespace WinSuperResolution.Models
{
    public sealed class RegistryTarget
    {
        public string RelativePath { get; set; }
        public int PrimarySurfaceWidth { get; set; }
        public int PrimarySurfaceHeight { get; set; }
        public int ActiveSignalWidth { get; set; }
        public int ActiveSignalHeight { get; set; }

        public bool HasActiveSignal
        {
            get { return ActiveSignalWidth > 0 && ActiveSignalHeight > 0; }
        }
    }

    public sealed class DisplayConfigurationRecord
    {
        public DisplayConfigurationRecord()
        {
            RegistryTargets = new List<RegistryTarget>();
            ConnectionStatus = ConnectionStatus.Historical;
            MatchStatus = MatchStatus.Unmatched;
            ValidationStatus = ValidationStatus.Blocked;
            CalculationBasis = CalculationBasis.Unavailable;
        }

        public string ConfigurationKey { get; set; }
        public IList<RegistryTarget> RegistryTargets { get; private set; }
        public int PrimarySurfaceWidth { get; set; }
        public int PrimarySurfaceHeight { get; set; }
        public int ActiveSignalWidth { get; set; }
        public int ActiveSignalHeight { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        public MatchStatus MatchStatus { get; set; }
        public ValidationStatus ValidationStatus { get; set; }
        public CalculationBasis CalculationBasis { get; set; }
        public string ScanWarning { get; set; }
        public LiveDisplayInfo LiveDisplay { get; set; }
        public string CorrelationEvidence { get; set; }
        public int DuplicateCandidateCount { get; set; }
        public bool SupersededByExactMatch { get; set; }
        public string ConnectionStatusText { get; set; }
        public string MatchStatusText { get; set; }
        public string PrimarySurfaceDisplayText { get; set; }
        public string ActiveSignalDisplayText { get; set; }

        public string PrimarySurfaceText
        {
            get { return FormatSize(PrimarySurfaceWidth, PrimarySurfaceHeight); }
        }

        public string ActiveSignalText
        {
            get { return FormatSize(ActiveSignalWidth, ActiveSignalHeight); }
        }

        public bool HasPrimarySurface
        {
            get { return PrimarySurfaceWidth > 0 && PrimarySurfaceHeight > 0; }
        }

        public bool HasActiveSignal
        {
            get { return ActiveSignalWidth > 0 && ActiveSignalHeight > 0; }
        }

        public bool CanManageCurrentState
        {
            get { return ConnectionStatus == ConnectionStatus.Active && MatchStatus == MatchStatus.Exact && LiveDisplay != null; }
        }

        public bool CanApplyVirtualCapability
        {
            get { return (ValidationStatus == ValidationStatus.Ready || ValidationStatus == ValidationStatus.Warning) && ConnectionStatus != ConnectionStatus.Conflicted && !SupersededByExactMatch; }
        }

        public string DisplayIdentity
        {
            get
            {
                if (LiveDisplay != null)
                {
                    string name = !string.IsNullOrEmpty(LiveDisplay.FriendlyName) ? LiveDisplay.FriendlyName : ConfigurationKey;
                    string device = string.IsNullOrEmpty(LiveDisplay.DeviceName) ? string.Empty : " [" + LiveDisplay.DeviceName + "]";
                    string connection = string.IsNullOrEmpty(LiveDisplay.ConnectionTechnology) || LiveDisplay.ConnectionTechnology == "Unknown"
                        ? string.Empty
                        : " · " + LiveDisplay.ConnectionTechnology;
                    return name + device + connection;
                }
                return ConfigurationKey;
            }
        }

        private static string FormatSize(int width, int height)
        {
            return width > 0 && height > 0 ? width + " x " + height : "Unavailable";
        }
    }
}
