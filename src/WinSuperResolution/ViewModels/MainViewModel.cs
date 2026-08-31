using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using WinSuperResolution.Models;
using WinSuperResolution.Resources;
using WinSuperResolution.Services;

namespace WinSuperResolution.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly DiagnosticsService _diagnostics;
        private readonly DisplayCatalogService _catalogService;
        private readonly ResolutionPlanService _planService;
        private readonly RegistryCapabilityService _capabilityService;
        private readonly DisplayModeService _displayModeService;
        private readonly ExperimentalScaleService _scaleService;
        private readonly PortableSettingsService _settingsService;
        private DisplayConfigurationRecord _selectedRecord;
        private DisplayMode _selectedMode;
        private int _selectedScalePercent;
        private int _selectedMagnification;
        private string _selectedLanguage;
        private string _statusText;
        private string _planSummary;
        private string _scaleAvailability;
        private string _currentModeAvailability;
        private string _lastOperationSummary;
        private bool _hasOperationInSession;
        private LocalizedStrings _ui;

        public MainViewModel()
        {
            _diagnostics = new DiagnosticsService();
            JournalService journals = new JournalService();
            _catalogService = new DisplayCatalogService(_diagnostics);
            _planService = new ResolutionPlanService();
            _capabilityService = new RegistryCapabilityService(journals, _diagnostics);
            _displayModeService = new DisplayModeService(journals);
            _scaleService = new ExperimentalScaleService(journals, _diagnostics);
            _settingsService = new PortableSettingsService();
            Records = new ObservableCollection<DisplayConfigurationRecord>();
            CurrentModes = new ObservableCollection<DisplayMode>();
            AvailableScalePercentages = new ObservableCollection<int>();
            Languages = Strings.SupportedCultures;
            _selectedMagnification = 150;
            _selectedLanguage = _settingsService.LoadLanguage();
            if (!Strings.IsSupported(_selectedLanguage))
                _selectedLanguage = Strings.DefaultCulture;
            _ui = Strings.ForCulture(_selectedLanguage);
            _statusText = Ui["Ready"];
            _planSummary = Ui["NoPlan"];
            _scaleAvailability = Ui["ScaleNoSelection"];
            _currentModeAvailability = Ui["ModeNoSelection"];
            _lastOperationSummary = Ui["NoOperationYet"];
        }

        public ObservableCollection<DisplayConfigurationRecord> Records { get; private set; }
        public ObservableCollection<DisplayMode> CurrentModes { get; private set; }
        public ObservableCollection<int> AvailableScalePercentages { get; private set; }
        public IList<string> Languages { get; private set; }
        public LocalizedStrings Ui { get { return _ui; } }
        public string RecoveryDataPath { get { return AppPaths.DataRoot; } }

        public DisplayConfigurationRecord SelectedRecord
        {
            get { return _selectedRecord; }
            set
            {
                if (_selectedRecord == value)
                    return;
                _selectedRecord = value;
                RefreshCurrentState();
                OnPropertyChanged("SelectedRecord");
                OnPropertyChanged("SelectedSummary");
                OnPropertyChanged("CanManageCurrentState");
                OnPropertyChanged("CurrentStateSummary");
            }
        }

        public DisplayMode SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                _selectedMode = value;
                OnPropertyChanged("SelectedMode");
            }
        }

        public int SelectedMagnification
        {
            get { return _selectedMagnification; }
            set
            {
                if (_selectedMagnification == value)
                    return;
                _selectedMagnification = value;
                OnPropertyChanged("SelectedMagnification");
            }
        }

        public int SelectedScalePercent
        {
            get { return _selectedScalePercent; }
            set
            {
                _selectedScalePercent = value;
                OnPropertyChanged("SelectedScalePercent");
            }
        }

        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (!Strings.IsSupported(value) || _selectedLanguage == value)
                    return;
                _selectedLanguage = value;
                _ui = Strings.ForCulture(value);
                _settingsService.SaveLanguage(value);
                OnPropertyChanged("SelectedLanguage");
                OnPropertyChanged("Ui");
                if (!_hasOperationInSession)
                    LastOperationSummary = Ui["NoOperationYet"];
                Refresh();
            }
        }

        public string SelectedSummary
        {
            get
            {
                if (SelectedRecord == null)
                    return Ui["SelectConfiguration"];
                return string.Format(Ui["SelectedSummary"],
                    SelectedRecord.DisplayIdentity,
                    SelectedRecord.RegistryTargets.Count,
                    SelectedRecord.MatchStatusText,
                    SelectedRecord.ConnectionStatusText,
                    LocalizeEvidence(SelectedRecord.CorrelationEvidence),
                    LocalizeWarning(SelectedRecord.ScanWarning));
            }
        }

        public string CurrentStateSummary
        {
            get
            {
                if (SelectedRecord == null || SelectedRecord.LiveDisplay == null)
                    return Ui["NoLiveDisplay"];
                LiveDisplayInfo display = SelectedRecord.LiveDisplay;
                return string.Format(Ui["CurrentStateSummary"],
                    SelectedRecord.PrimarySurfaceText,
                    display.CurrentModeText,
                    display.ScaleText);
            }
        }

        public bool CanManageCurrentState
        {
            get { return SelectedRecord != null && SelectedRecord.CanManageCurrentState; }
        }

        public bool CanApplyExperimentalScale
        {
            get { return AvailableScalePercentages.Count > 0 && SelectedScalePercent > 0; }
        }

        public string ScaleAvailability
        {
            get { return _scaleAvailability; }
            private set
            {
                _scaleAvailability = value;
                OnPropertyChanged("ScaleAvailability");
            }
        }

        public string CurrentModeAvailability
        {
            get { return _currentModeAvailability; }
            private set
            {
                _currentModeAvailability = value;
                OnPropertyChanged("CurrentModeAvailability");
            }
        }

        public string LastOperationSummary
        {
            get { return _lastOperationSummary; }
            private set
            {
                _lastOperationSummary = value;
                OnPropertyChanged("LastOperationSummary");
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        public string PlanSummary
        {
            get { return _planSummary; }
            private set
            {
                _planSummary = value;
                OnPropertyChanged("PlanSummary");
            }
        }

        public void Refresh()
        {
            try
            {
                IList<DisplayConfigurationRecord> records = _catalogService.Scan();
                Records.Clear();
                foreach (DisplayConfigurationRecord record in records)
                {
                    LocalizeRecordPresentation(record);
                    Records.Add(record);
                }
                SelectedRecord = Records.Count > 0 ? Records[0] : null;
                PlanSummary = Ui["NoPlan"];
                StatusText = string.Format(Ui["ScanComplete"], Records.Count, CountTargets());
            }
            catch (Exception exception)
            {
                StatusText = Ui["ScanFailed"] + exception.Message;
                _diagnostics.Write(StatusText);
            }
        }

        public void BuildPlan()
        {
            try
            {
                ResolutionPlan plan = BuildSelectedPlan();
                PlanSummary = plan.Summary + " " + Ui["PreviewOnly"];
                StatusText = Ui["PlanBuilt"];
            }
            catch (Exception exception)
            {
                PlanSummary = Ui["PlanUnavailable"] + exception.Message;
                StatusText = Ui["PlanValidationFailed"];
            }
        }

        public OperationResult ApplySelectedCapability()
        {
            try
            {
                OperationResult result = _capabilityService.Apply(BuildSelectedPlan());
                RecordOperation(result);
                return result;
            }
            catch (Exception exception)
            {
                return new OperationResult { Succeeded = false, Message = exception.Message };
            }
        }

        public IList<ResolutionPlan> GetSelectedCapabilityPreview()
        {
            return new List<ResolutionPlan> { BuildSelectedPlan() };
        }

        public OperationResult ApplyAllCapabilities()
        {
            try
            {
                List<ResolutionPlan> plans = new List<ResolutionPlan>();
                foreach (DisplayConfigurationRecord record in Records)
                {
                    if (record.ValidationStatus == ValidationStatus.Ready || record.ValidationStatus == ValidationStatus.Warning)
                        plans.Add(_planService.Build(record, SelectedMagnification));
                }
                OperationResult result = _capabilityService.ApplyBatch(plans);
                RecordOperation(result);
                return result;
            }
            catch (Exception exception)
            {
                return new OperationResult { Succeeded = false, Message = exception.Message };
            }
        }

        public IList<ResolutionPlan> GetAllCapabilityPreview()
        {
            List<ResolutionPlan> plans = new List<ResolutionPlan>();
            foreach (DisplayConfigurationRecord record in Records)
            {
                if (record.ValidationStatus == ValidationStatus.Ready || record.ValidationStatus == ValidationStatus.Warning)
                    plans.Add(_planService.Build(record, SelectedMagnification));
            }
            return plans;
        }

        public OperationResult RestoreLatestCapability()
        {
            OperationResult result = _capabilityService.RestoreLatest();
            RecordOperation(result);
            return result;
        }

        public OperationResult ApplyCurrentMode()
        {
            if (!CanManageCurrentState)
                return new OperationResult { Succeeded = false, Message = "Current mode requires an Active + Exact display association." };
            OperationResult result = _displayModeService.ApplyWithSnapshot(SelectedMode);
            RecordOperation(result);
            return result;
        }

        public OperationResult ConfirmCurrentMode()
        {
            OperationResult result = _displayModeService.ConfirmPending();
            RecordOperation(result);
            return result;
        }

        public OperationResult RestoreCurrentMode()
        {
            OperationResult result = _displayModeService.RestorePending();
            RecordOperation(result);
            Refresh();
            return result;
        }

        public OperationResult ApplyExperimentalScale()
        {
            OperationResult result = _scaleService.Apply(SelectedRecord, SelectedScalePercent);
            if (result.Succeeded)
                result.Message = Ui["ScaleApplied"];
            RecordOperation(result);
            return result;
        }

        public OperationResult RestoreLatestExperimentalScale()
        {
            OperationResult result = _scaleService.RestoreLatest();
            if (result.Succeeded)
                result.Message = Ui["ScaleRestored"];
            RecordOperation(result);
            return result;
        }

        public string BuildDiagnosticSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("WinSuperResolution v2 diagnostic");
            builder.AppendLine("Registered configuration roots: " + Records.Count);
            builder.AppendLine("Writable targets: " + CountTargets());
            foreach (DisplayConfigurationRecord record in Records)
                builder.AppendLine(record.ConfigurationKey + " | " + record.PrimarySurfaceText + " | " + record.ActiveSignalText + " | " + record.ValidationStatus + " | " + record.MatchStatus);
            return builder.ToString();
        }

        private ResolutionPlan BuildSelectedPlan()
        {
            return _planService.Build(SelectedRecord, SelectedMagnification);
        }

        private void RefreshCurrentState()
        {
            CurrentModes.Clear();
            AvailableScalePercentages.Clear();
            SelectedMode = null;
            SelectedScalePercent = 0;
            CurrentModeAvailability = LocalizeCurrentModeAvailability();
            if (SelectedRecord != null && SelectedRecord.CanManageCurrentState)
            {
                foreach (DisplayMode mode in _displayModeService.EnumerateModes(SelectedRecord))
                {
                    mode.ModeKindText = mode.IsVirtualDesktopMode ? Ui["ModeKindVirtualDesktop"] : Ui["ModeKindDriver"];
                    CurrentModes.Add(mode);
                    if (mode.IsCurrent)
                        SelectedMode = mode;
                }
                if (SelectedMode == null && CurrentModes.Count > 0)
                    SelectedMode = CurrentModes[0];
                CurrentModeAvailability = CurrentModes.Count > 0 ? Ui["ModeAvailable"] : Ui["ModeNoModes"];
            }
            foreach (int scale in _scaleService.GetAvailableScalePercentages(SelectedRecord))
                AvailableScalePercentages.Add(scale);
            if (AvailableScalePercentages.Count > 0)
            {
                int currentScale = SelectedRecord != null && SelectedRecord.LiveDisplay != null ? SelectedRecord.LiveDisplay.CurrentScalePercent : 0;
                SelectedScalePercent = AvailableScalePercentages.Contains(currentScale) ? currentScale : AvailableScalePercentages[0];
            }
            ScaleAvailability = LocalizeScaleAvailability(_scaleService.GetAvailabilityStatus(SelectedRecord));
            OnPropertyChanged("CanApplyExperimentalScale");
            OnPropertyChanged("CanManageCurrentState");
        }

        private string LocalizeCurrentModeAvailability()
        {
            if (SelectedRecord == null)
                return Ui["ModeNoSelection"];
            if (SelectedRecord.ConnectionStatus != ConnectionStatus.Active)
                return Ui["ModeRequiresActive"];
            if (SelectedRecord.MatchStatus != MatchStatus.Exact)
                return Ui["ModeRequiresExact"];
            if (SelectedRecord.LiveDisplay == null)
                return Ui["ModeNoLiveDisplay"];
            return Ui["ModeNoModes"];
        }

        private void RecordOperation(OperationResult result)
        {
            _hasOperationInSession = true;
            StatusText = result.Message;
            LastOperationSummary = BuildOperationSummary(result);
        }

        private string BuildOperationSummary(OperationResult result)
        {
            if (result == null)
                return string.Empty;
            StringBuilder builder = new StringBuilder();
            builder.Append(result.Succeeded ? Ui["OperationSucceeded"] : Ui["OperationFailed"]);
            if (!string.IsNullOrEmpty(result.BackupPath))
                builder.AppendLine().Append(Ui["BackupPathLabel"]).Append(result.BackupPath);
            if (!string.IsNullOrEmpty(result.JournalPath))
                builder.AppendLine().Append(Ui["JournalPathLabel"]).Append(result.JournalPath);
            if (result.RestartRequired)
                builder.AppendLine().Append(Ui["RestartRequiredNotice"]);
            return builder.ToString();
        }

        private string LocalizeScaleAvailability(ScaleAvailabilityStatus status)
        {
            switch (status)
            {
                case ScaleAvailabilityStatus.NoSelection: return Ui["ScaleNoSelection"];
                case ScaleAvailabilityStatus.RequiresActiveDisplay: return Ui["ScaleRequiresActiveDisplay"];
                case ScaleAvailabilityStatus.CurrentScaleUnavailable: return Ui["ScaleCurrentUnavailable"];
                case ScaleAvailabilityStatus.NoCompatibleSettingsTarget: return Ui["ScaleNoCompatibleSettingsTarget"];
                default: return Ui["ScaleAvailable"];
            }
        }

        private void LocalizeRecordPresentation(DisplayConfigurationRecord record)
        {
            record.ConnectionStatusText = LocalizeConnectionStatus(record.ConnectionStatus);
            record.MatchStatusText = LocalizeMatchStatus(record.MatchStatus);
            record.PrimarySurfaceDisplayText = record.HasPrimarySurface ? record.PrimarySurfaceText : Ui["Unavailable"];
            record.ActiveSignalDisplayText = record.HasActiveSignal ? record.ActiveSignalText : Ui["Unavailable"];
        }

        private string LocalizeConnectionStatus(ConnectionStatus status)
        {
            switch (status)
            {
                case ConnectionStatus.Active: return Ui["ConnectionActive"];
                case ConnectionStatus.Historical: return Ui["ConnectionHistorical"];
                case ConnectionStatus.Inactive: return Ui["ConnectionInactive"];
                default: return Ui["ConnectionUnknown"];
            }
        }

        private string LocalizeMatchStatus(MatchStatus status)
        {
            switch (status)
            {
                case MatchStatus.Exact: return Ui["MatchExact"];
                case MatchStatus.Candidate: return Ui["MatchCandidate"];
                case MatchStatus.Ambiguous: return Ui["MatchAmbiguous"];
                default: return Ui["MatchUnmatched"];
            }
        }

        private string LocalizeEvidence(string evidence)
        {
            if (evidence == "Unique EDID/monitor identity evidence and current-mode evidence matched.") return Ui["EvidenceExact"];
            if (evidence == "Unique active topology and current-mode resolution evidence matched; the registry key has no stable monitor token.") return Ui["EvidenceTopologyUnique"];
            if (evidence == "Current-mode resolution matches, but the registry key lacks a unique monitor instance token.") return Ui["EvidenceCandidate"];
            if (evidence == "No active Windows display has compatible resolution evidence.") return Ui["EvidenceUnmatched"];
            if (evidence == "Multiple active displays match only resolution evidence.") return Ui["EvidenceAmbiguous"];
            return evidence;
        }

        private string LocalizeWarning(string warning)
        {
            if (warning == "Candidate live association only. Current mode and experimental scaling controls stay disabled until an Exact match is proven.") return Ui["WarningCandidate"];
            if (warning == "Historical or uncorrelated registry configuration. It remains eligible for virtual-capability planning, but current mode and scale controls are disabled.") return Ui["WarningUnmatched"];
            if (warning == "Ambiguous live association. The record is not treated as the current display.") return Ui["WarningAmbiguous"];
            if (warning == "No writable PrimSurfSize target was found.") return Ui["WarningNoTarget"];
            return warning;
        }

        private int CountTargets()
        {
            int count = 0;
            foreach (DisplayConfigurationRecord record in Records)
                count += record.RegistryTargets.Count;
            return count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
