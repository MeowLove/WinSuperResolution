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
        private readonly DiagnosticExportService _diagnosticExportService;
        private readonly DisplayCacheResetService _displayCacheResetService;
        private readonly EnvironmentCompatibilityService _environmentCompatibilityService;
        private readonly VirtualDesktopModeService _virtualDesktopModes;
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
        private LocalizedStrings _ui;
        private EnvironmentCompatibilitySnapshot _compatibility;
        private OperationResult _lastLocalizedOperation;
        private string _lastSuccessMessageKey;
        private string _lastFailureMessageKey;
        private DiagnosticExportResult _lastDiagnosticExport;

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
            _diagnosticExportService = new DiagnosticExportService();
            _displayCacheResetService = new DisplayCacheResetService(journals, _diagnostics);
            _environmentCompatibilityService = new EnvironmentCompatibilityService();
            _virtualDesktopModes = new VirtualDesktopModeService();
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
            RefreshCompatibility();
        }

        public ObservableCollection<DisplayConfigurationRecord> Records { get; private set; }
        public ObservableCollection<DisplayMode> CurrentModes { get; private set; }
        public ObservableCollection<int> AvailableScalePercentages { get; private set; }
        public IList<string> Languages { get; private set; }
        public LocalizedStrings Ui { get { return _ui; } }
        public string RecoveryDataPath { get { return AppPaths.DataRoot; } }

        public string CompatibilityStatusText
        {
            get
            {
                if (_compatibility == null)
                    return Ui["CompatibilityExperimental"];
                switch (_compatibility.Status)
                {
                    case EnvironmentCompatibilityStatus.Unsupported: return Ui["CompatibilityUnsupported"];
                    case EnvironmentCompatibilityStatus.CanTry: return Ui["CompatibilityCanTry"];
                    default: return Ui["CompatibilityExperimental"];
                }
            }
        }

        public string CompatibilityStatusBackground
        {
            get
            {
                if (_compatibility == null || _compatibility.Status == EnvironmentCompatibilityStatus.Experimental)
                    return "#FFF3CD";
                return _compatibility.Status == EnvironmentCompatibilityStatus.Unsupported ? "#FDE2E1" : "#DCF2E3";
            }
        }

        public string CompatibilityStatusForeground
        {
            get
            {
                if (_compatibility == null || _compatibility.Status == EnvironmentCompatibilityStatus.Experimental)
                    return "#7A4B00";
                return _compatibility.Status == EnvironmentCompatibilityStatus.Unsupported ? "#9B2C2C" : "#1F6B3A";
            }
        }

        public string CompatibilitySummary
        {
            get
            {
                if (SelectedRecord == null || SelectedRecord.LiveDisplay == null)
                    return Ui["CompatibilityNoSelectionReason"];
                if (_compatibility == null || _compatibility.Status == EnvironmentCompatibilityStatus.Experimental)
                    return Ui["CompatibilityExperimentalReason"];
                return _compatibility.Status == EnvironmentCompatibilityStatus.Unsupported
                    ? Ui["CompatibilityUnsupportedReason"]
                    : Ui["CompatibilityCanTryReason"];
            }
        }
        public string CompatibilitySystem { get { return _compatibility == null ? Ui["Unavailable"] : _compatibility.WindowsSummary; } }
        public string CompatibilityProcessor { get { return _compatibility == null ? Ui["Unavailable"] : _compatibility.ProcessorSummary; } }
        public string CompatibilityGraphics { get { return _compatibility == null ? Ui["Unavailable"] : _compatibility.GraphicsSummary; } }
        public string CompatibilityDisplayPath
        {
            get
            {
                if (SelectedRecord == null || SelectedRecord.LiveDisplay == null)
                    return Ui["CompatibilityNoSelectionReason"];
                return _compatibility != null && _compatibility.Status != EnvironmentCompatibilityStatus.Unsupported
                    ? string.Format(Ui["CompatibilityPathSupported"], SelectedRecord.LiveDisplay.DeviceName)
                    : string.Format(Ui["CompatibilityPathUnavailable"], SelectedRecord.LiveDisplay.DeviceName);
            }
        }

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
                OnPropertyChanged("CanApplySelectedCapability");
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
                Refresh();
                RefreshLastOperationPresentation();
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

        public bool CanApplySelectedCapability
        {
            get { return SelectedRecord != null && SelectedRecord.CanApplyVirtualCapability; }
        }

        public bool CanApplyAllCapabilities
        {
            get
            {
                foreach (DisplayConfigurationRecord record in Records)
                {
                    if (record.CanApplyVirtualCapability)
                        return true;
                }
                return false;
            }
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
                DisplayConfigurationRecord preferredRecord = null;
                foreach (DisplayConfigurationRecord record in Records)
                {
                    if (record.CanManageCurrentState)
                    {
                        preferredRecord = record;
                        break;
                    }
                }
                SelectedRecord = preferredRecord ?? (Records.Count > 0 ? Records[0] : null);
                PlanSummary = Ui["NoPlan"];
                StatusText = string.Format(Ui["ScanComplete"], Records.Count, CountTargets());
                OnPropertyChanged("CanApplyAllCapabilities");
            }
            catch (Exception exception)
            {
                StatusText = Ui["ScanFailed"];
                _diagnostics.Write("Display scan failed: " + exception);
            }
        }

        public void BuildPlan()
        {
            try
            {
                ResolutionPlan plan = BuildSelectedPlan();
                PlanSummary = DescribePlan(plan) + " " + Ui["PreviewOnly"];
                StatusText = Ui["PlanBuilt"];
            }
            catch (Exception exception)
            {
                PlanSummary = Ui["PlanUnavailable"];
                StatusText = Ui["PlanValidationFailed"];
                _diagnostics.Write("Capability plan generation failed: " + exception);
            }
        }

        public OperationResult ApplySelectedCapability()
        {
            try
            {
                return RecordLocalizedResult(_capabilityService.Apply(BuildSelectedPlan()), "CapabilityApplied", "CapabilityFailed");
            }
            catch (Exception exception)
            {
                _diagnostics.Write("Apply selected capability failed before execution: " + exception);
                return RecordLocalizedResult(new OperationResult { Succeeded = false, Message = exception.Message }, "CapabilityApplied", "CapabilityFailed");
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
                    if (record.CanApplyVirtualCapability)
                        plans.Add(_planService.Build(record, SelectedMagnification));
                }
                return RecordLocalizedResult(_capabilityService.ApplyBatch(plans), "CapabilityApplied", "CapabilityFailed");
            }
            catch (Exception exception)
            {
                _diagnostics.Write("Apply all capabilities failed before execution: " + exception);
                return RecordLocalizedResult(new OperationResult { Succeeded = false, Message = exception.Message }, "CapabilityApplied", "CapabilityFailed");
            }
        }

        public IList<ResolutionPlan> GetAllCapabilityPreview()
        {
            List<ResolutionPlan> plans = new List<ResolutionPlan>();
            foreach (DisplayConfigurationRecord record in Records)
            {
                if (record.CanApplyVirtualCapability)
                    plans.Add(_planService.Build(record, SelectedMagnification));
            }
            return plans;
        }

        public OperationResult RestoreLatestCapability()
        {
            return RecordLocalizedResult(_capabilityService.RestoreLatest(), "CapabilityRestored", "CapabilityRestoreFailed");
        }

        public OperationResult ApplyCurrentMode()
        {
            if (!CanManageCurrentState)
                return RecordLocalizedResult(new OperationResult { Succeeded = false, Message = "Current mode requires an Active + Exact display association." }, "ModeApplied", "ModeFailed");
            return RecordLocalizedResult(_displayModeService.ApplyWithSnapshot(SelectedMode), "ModeApplied", "ModeFailed");
        }

        public OperationResult ConfirmCurrentMode()
        {
            return RecordLocalizedResult(_displayModeService.ConfirmPending(), "ModeRetained", "ModeRetainFailed");
        }

        public OperationResult RestoreCurrentMode()
        {
            OperationResult result = RecordLocalizedResult(_displayModeService.RestorePending(), "ModeRestored", "ModeRestoreFailed");
            Refresh();
            return result;
        }

        public OperationResult ApplyExperimentalScale()
        {
            return RecordLocalizedResult(_scaleService.Apply(SelectedRecord, SelectedScalePercent), "ScaleApplied", "ScaleFailed");
        }

        public OperationResult RestoreLatestExperimentalScale()
        {
            return RecordLocalizedResult(_scaleService.RestoreLatest(), "ScaleRestored", "ScaleFailed");
        }

        public string BuildDiagnosticSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("WinSuperResolution v2.5 diagnostic");
            builder.AppendLine("Registered configuration roots: " + Records.Count);
            builder.AppendLine("Writable targets: " + CountTargets());
            foreach (DisplayConfigurationRecord record in Records)
            {
                builder.AppendLine("Configuration: " + record.ConfigurationKey + " | primary=" + record.PrimarySurfaceText + " | signal=" + record.ActiveSignalText + " | validation=" + record.ValidationStatus + " | connection=" + record.ConnectionStatus + " | match=" + record.MatchStatus + " | duplicateCandidates=" + record.DuplicateCandidateCount + " | supersededByExact=" + record.SupersededByExactMatch);
                if (record.LiveDisplay != null)
                {
                    LiveDisplayInfo display = record.LiveDisplay;
                    builder.AppendLine("  LiveDisplay: device=" + display.DeviceName + " | monitorId=" + display.MonitorDeviceId + " | monitorKey=" + display.MonitorDeviceKey + " | devicePath=" + display.MonitorDevicePath + " | edid=" + display.EdidManufacturer + ":" + display.EdidProductCode + " | connection=" + display.ConnectionTechnology + " | mode=" + display.CurrentModeText + " | scale=" + display.ScaleText);
                }
                foreach (RegistryTarget target in record.RegistryTargets)
                    builder.AppendLine("  Target: " + target.RelativePath + " | primary=" + target.PrimarySurfaceWidth + "x" + target.PrimarySurfaceHeight + " | active=" + target.ActiveSignalWidth + "x" + target.ActiveSignalHeight);
                if (!string.IsNullOrEmpty(record.CorrelationEvidence))
                    builder.AppendLine("  Correlation: " + record.CorrelationEvidence);
                if (!string.IsNullOrEmpty(record.ScanWarning))
                    builder.AppendLine("  Warning: " + record.ScanWarning);
                EnvironmentCompatibilitySnapshot compatibility = _environmentCompatibilityService.Inspect(record, _virtualDesktopModes);
                builder.AppendLine("  Compatibility: status=" + compatibility.Status + " | system=" + compatibility.WindowsSummary + " | processor=" + compatibility.ProcessorSummary + " | graphics=" + compatibility.GraphicsSummary + " | path=" + compatibility.PathSummary + " | reason=" + compatibility.Reason);
            }
            return builder.ToString();
        }

        internal DiagnosticExportResult ExportDiagnosticPackage()
        {
            DiagnosticExportResult result = _diagnosticExportService.Export(BuildDiagnosticSummary());
            _lastDiagnosticExport = result;
            if (result.Succeeded)
            {
                StatusText = Ui["DiagnosticExported"] + result.ArchivePath;
                LastOperationSummary = StatusText;
            }
            else
            {
                StatusText = Ui["DiagnosticExportFailed"];
                LastOperationSummary = StatusText;
            }
            return result;
        }

        public OperationResult ResetDisplayCache()
        {
            return RecordLocalizedResult(_displayCacheResetService.Reset(), "DisplayCacheResetSuccess", "DisplayCacheFailedDetail");
        }

        private ResolutionPlan BuildSelectedPlan()
        {
            return _planService.Build(SelectedRecord, SelectedMagnification);
        }

        private OperationResult RecordLocalizedResult(OperationResult result, string successKey, string failureKey)
        {
            if (result == null)
                result = new OperationResult { Succeeded = false, Message = "The operation returned no result." };
            if (!result.Succeeded)
                _diagnostics.Write("User-visible operation failure: " + result.Message);
            _lastLocalizedOperation = result;
            _lastSuccessMessageKey = successKey;
            _lastFailureMessageKey = failureKey;
            _lastDiagnosticExport = null;
            result.Message = result.Succeeded ? Ui[successKey] : Ui[failureKey];
            RecordOperation(result);
            return result;
        }

        private void RefreshLastOperationPresentation()
        {
            if (_lastLocalizedOperation != null)
            {
                _lastLocalizedOperation.Message = _lastLocalizedOperation.Succeeded ? Ui[_lastSuccessMessageKey] : Ui[_lastFailureMessageKey];
                LastOperationSummary = BuildOperationSummary(_lastLocalizedOperation);
                return;
            }
            if (_lastDiagnosticExport != null)
            {
                LastOperationSummary = _lastDiagnosticExport.Succeeded
                    ? Ui["DiagnosticExported"] + _lastDiagnosticExport.ArchivePath
                    : Ui["DiagnosticExportFailed"];
                return;
            }
            LastOperationSummary = Ui["NoOperationYet"];
        }

        private string DescribePlan(ResolutionPlan plan)
        {
            string basis = plan.Basis == CalculationBasis.ActiveSize ? Ui["PlanBasisActiveSize"] : Ui["PlanBasisPrimSurfSize"];
            return string.Format(Ui["PlanSummary"], plan.Magnification, basis, plan.BaseWidth, plan.BaseHeight, plan.TargetWidth, plan.TargetHeight, plan.Mutations.Count);
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
            RefreshCompatibility();
            OnPropertyChanged("CanApplyExperimentalScale");
            OnPropertyChanged("CanManageCurrentState");
        }

        private void RefreshCompatibility()
        {
            _compatibility = _environmentCompatibilityService.Inspect(SelectedRecord, _virtualDesktopModes);
            OnPropertyChanged("CompatibilityStatusText");
            OnPropertyChanged("CompatibilityStatusBackground");
            OnPropertyChanged("CompatibilityStatusForeground");
            OnPropertyChanged("CompatibilitySummary");
            OnPropertyChanged("CompatibilitySystem");
            OnPropertyChanged("CompatibilityProcessor");
            OnPropertyChanged("CompatibilityGraphics");
            OnPropertyChanged("CompatibilityDisplayPath");
        }

        private string LocalizeCurrentModeAvailability()
        {
            if (SelectedRecord == null)
                return Ui["ModeNoSelection"];
            if (SelectedRecord.ConnectionStatus == ConnectionStatus.Conflicted)
                return Ui["ModeRequiresConflict"];
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
                case ConnectionStatus.Conflicted: return Ui["ConnectionConflicted"];
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
            if (evidence == "Current-mode resolution matches; stable monitor identity will be evaluated across all registered candidates.") return Ui["EvidenceCandidate"];
            if (evidence == "No active Windows display has compatible resolution evidence.") return Ui["EvidenceUnmatched"];
            if (evidence == "Multiple active displays match only resolution evidence.") return Ui["EvidenceAmbiguous"];
            if (evidence == "Multiple registered configuration roots match the same active Windows display by resolution only.") return Ui["EvidenceDuplicate"];
            return evidence;
        }

        private string LocalizeWarning(string warning)
        {
            if (warning == "Candidate live association only. Current desktop mode, experimental scaling, and virtual-capability writes stay disabled until an Exact match is proven.") return Ui["WarningCandidate"];
            if (warning == "Historical or uncorrelated registry configuration. It remains eligible for virtual-capability planning, but current mode and scale controls are disabled.") return Ui["WarningUnmatched"];
            if (warning == "Ambiguous live association. The record is not treated as the current display.") return Ui["WarningAmbiguous"];
            if (warning == "Duplicate candidate configuration. Virtual-resolution capability changes are disabled until the current registry configuration can be identified.") return Ui["WarningDuplicate"];
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
