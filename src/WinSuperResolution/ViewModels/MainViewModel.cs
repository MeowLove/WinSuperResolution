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
        private string _lastOperationSummary;
        private LocalizedStrings _ui;

        public MainViewModel()
        {
            _diagnostics = new DiagnosticsService();
            JournalService journals = new JournalService();
            _catalogService = new DisplayCatalogService(_diagnostics);
            _planService = new ResolutionPlanService();
            _capabilityService = new RegistryCapabilityService(journals, _diagnostics);
            _displayModeService = new DisplayModeService(journals);
            _scaleService = new ExperimentalScaleService(journals);
            _settingsService = new PortableSettingsService();
            Records = new ObservableCollection<DisplayConfigurationRecord>();
            CurrentModes = new ObservableCollection<DisplayMode>();
            AvailableScalePercentages = new ObservableCollection<int>();
            MagnificationOptions = new List<int>();
            for (int value = 100; value <= 350; value += 10)
                MagnificationOptions.Add(value);
            Languages = Strings.SupportedCultures;
            _selectedMagnification = 150;
            _selectedLanguage = _settingsService.LoadLanguage();
            if (!Strings.IsSupported(_selectedLanguage))
                _selectedLanguage = Strings.DefaultCulture;
            _ui = Strings.ForCulture(_selectedLanguage);
            _statusText = Ui["Ready"];
            _planSummary = Ui["NoPlan"];
            _scaleAvailability = Ui["ScaleNoSelection"];
            _lastOperationSummary = Ui["NoOperationYet"];
        }

        public ObservableCollection<DisplayConfigurationRecord> Records { get; private set; }
        public ObservableCollection<DisplayMode> CurrentModes { get; private set; }
        public ObservableCollection<int> AvailableScalePercentages { get; private set; }
        public IList<int> MagnificationOptions { get; private set; }
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
                OnPropertyChanged("SelectedSummary");
                OnPropertyChanged("CurrentStateSummary");
                ScaleAvailability = LocalizeScaleAvailability(_scaleService.GetAvailabilityStatus(SelectedRecord));
                if (string.IsNullOrEmpty(LastOperationSummary) || LastOperationSummary == "No operation has been recorded in this session.")
                    LastOperationSummary = Ui["NoOperationYet"];
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
                    SelectedRecord.MatchStatus,
                    SelectedRecord.ConnectionStatus,
                    SelectedRecord.CorrelationEvidence,
                    SelectedRecord.ScanWarning);
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
            get { return CanManageCurrentState && AvailableScalePercentages.Count > 0 && SelectedScalePercent > 0; }
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
                    Records.Add(record);
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
            if (SelectedRecord != null && SelectedRecord.CanManageCurrentState)
            {
                foreach (DisplayMode mode in _displayModeService.EnumerateModes(SelectedRecord.LiveDisplay.DeviceName))
                    CurrentModes.Add(mode);
                if (CurrentModes.Count > 0)
                    SelectedMode = CurrentModes[0];
            }
            foreach (int scale in _scaleService.GetAvailableScalePercentages(SelectedRecord))
                AvailableScalePercentages.Add(scale);
            if (AvailableScalePercentages.Count > 0)
                SelectedScalePercent = AvailableScalePercentages[0];
            ScaleAvailability = LocalizeScaleAvailability(_scaleService.GetAvailabilityStatus(SelectedRecord));
            OnPropertyChanged("CanApplyExperimentalScale");
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
                case ScaleAvailabilityStatus.RequiresExactMatch: return Ui["ScaleRequiresExactMatch"];
                case ScaleAvailabilityStatus.CurrentScaleUnavailable: return Ui["ScaleCurrentUnavailable"];
                case ScaleAvailabilityStatus.NoVerifiedProfile: return Ui["ScaleNoVerifiedProfile"];
                default: return Ui["ScaleAvailable"];
            }
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
