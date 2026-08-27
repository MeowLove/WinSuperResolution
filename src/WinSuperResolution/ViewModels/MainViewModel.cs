using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using WinSuperResolution.Models;
using WinSuperResolution.Services;

namespace WinSuperResolution.ViewModels
{
    internal sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly DiagnosticsService _diagnostics;
        private readonly DisplayCatalogService _catalogService;
        private readonly ResolutionPlanService _planService;
        private DisplayConfigurationRecord _selectedRecord;
        private int _selectedMagnification;
        private string _statusText;
        private string _planSummary;

        internal MainViewModel()
        {
            _diagnostics = new DiagnosticsService();
            _catalogService = new DisplayCatalogService(_diagnostics);
            _planService = new ResolutionPlanService();
            Records = new ObservableCollection<DisplayConfigurationRecord>();
            MagnificationOptions = new List<int>();
            for (int value = 100; value <= 350; value += 10)
            {
                MagnificationOptions.Add(value);
            }

            _selectedMagnification = 150;
            _statusText = "Ready. Refresh to scan registered display configurations.";
            _planSummary = "No plan has been built.";
        }

        internal ObservableCollection<DisplayConfigurationRecord> Records { get; private set; }
        internal IList<int> MagnificationOptions { get; private set; }

        internal DisplayConfigurationRecord SelectedRecord
        {
            get { return _selectedRecord; }
            set
            {
                if (_selectedRecord == value)
                {
                    return;
                }

                _selectedRecord = value;
                OnPropertyChanged("SelectedRecord");
                OnPropertyChanged("SelectedSummary");
            }
        }

        internal int SelectedMagnification
        {
            get { return _selectedMagnification; }
            set
            {
                if (_selectedMagnification == value)
                {
                    return;
                }

                _selectedMagnification = value;
                OnPropertyChanged("SelectedMagnification");
            }
        }

        internal string SelectedSummary
        {
            get
            {
                if (SelectedRecord == null)
                {
                    return "Select a registered configuration to inspect its virtual-resolution capability.";
                }

                return string.Format("{0}\nTargets: {1}\nMatch: {2}; connection: {3}\n{4}",
                    SelectedRecord.ConfigurationKey,
                    SelectedRecord.RegistryTargets.Count,
                    SelectedRecord.MatchStatus,
                    SelectedRecord.ConnectionStatus,
                    SelectedRecord.ScanWarning);
            }
        }

        internal string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        internal string PlanSummary
        {
            get { return _planSummary; }
            set
            {
                _planSummary = value;
                OnPropertyChanged("PlanSummary");
            }
        }

        internal void Refresh()
        {
            try
            {
                IList<DisplayConfigurationRecord> records = _catalogService.Scan();
                Records.Clear();
                foreach (DisplayConfigurationRecord record in records)
                {
                    Records.Add(record);
                }

                SelectedRecord = Records.Count > 0 ? Records[0] : null;
                PlanSummary = "No plan has been built.";
                StatusText = string.Format("Read-only scan complete: {0} configuration root(s), {1} writable target(s).", Records.Count, CountTargets());
            }
            catch (Exception exception)
            {
                StatusText = "Scan failed: " + exception.Message;
                _diagnostics.Write(StatusText);
            }
        }

        internal void BuildPlan()
        {
            try
            {
                ResolutionPlan plan = _planService.Build(SelectedRecord, SelectedMagnification);
                PlanSummary = plan.Summary + " Read-only preview; no registry values were changed.";
                StatusText = "Resolution plan built successfully.";
            }
            catch (Exception exception)
            {
                PlanSummary = "Plan unavailable: " + exception.Message;
                StatusText = "Plan validation failed.";
            }
        }

        internal string BuildDiagnosticSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("WinSuperResolution v2 read-only diagnostic");
            builder.AppendLine("Registered configuration roots: " + Records.Count);
            builder.AppendLine("Writable targets: " + CountTargets());
            foreach (DisplayConfigurationRecord record in Records)
            {
                builder.AppendLine(record.ConfigurationKey + " | " + record.PrimarySurfaceText + " | " + record.ActiveSignalText + " | " + record.ValidationStatus);
            }

            return builder.ToString();
        }

        private int CountTargets()
        {
            int count = 0;
            foreach (DisplayConfigurationRecord record in Records)
            {
                count += record.RegistryTargets.Count;
            }

            return count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
