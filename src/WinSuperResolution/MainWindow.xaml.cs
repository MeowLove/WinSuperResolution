using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WinSuperResolution.Models;
using WinSuperResolution.Services;
using WinSuperResolution.ViewModels;
using WinSuperResolution.Windows;

namespace WinSuperResolution
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            ApplyLocalizedColumnHeaders();
            Loaded += MainWindow_Loaded;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Ui")
                ApplyLocalizedColumnHeaders();
        }

        private void ApplyLocalizedColumnHeaders()
        {
            if (DisplayGrid == null || _viewModel == null || DisplayGrid.Columns.Count < 5)
                return;
            DisplayGrid.Columns[0].Header = _viewModel.Ui["Status"];
            DisplayGrid.Columns[1].Header = _viewModel.Ui["Display"];
            DisplayGrid.Columns[2].Header = _viewModel.Ui["Surface"];
            DisplayGrid.Columns[3].Header = _viewModel.Ui["Signal"];
            DisplayGrid.Columns[4].Header = _viewModel.Ui["Association"];
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Refresh();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Refresh();
        }

        private void BuildPlanButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BuildPlan();
        }

        private void SetMagnificationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int magnification;
            if (button == null || !int.TryParse(button.Tag as string, out magnification))
                return;
            _viewModel.SelectedMagnification = magnification;
        }

        private void ApplySelectedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IList<ResolutionPlan> plans = _viewModel.GetSelectedCapabilityPreview();
                CapabilityConfirmationWindow confirmation = new CapabilityConfirmationWindow(_viewModel.Ui["ProductName"], _viewModel.Ui["ConfirmCapability"], plans, _viewModel.Ui["ApplySelected"], _viewModel.Ui["Cancel"]);
                confirmation.Owner = this;
                if (confirmation.ShowDialog() == true)
                    ShowResult(_viewModel.ApplySelectedCapability());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, _viewModel.Ui["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IList<ResolutionPlan> plans = _viewModel.GetAllCapabilityPreview();
                CapabilityConfirmationWindow confirmation = new CapabilityConfirmationWindow(_viewModel.Ui["ProductName"], _viewModel.Ui["ConfirmAll"], plans, _viewModel.Ui["ApplyAll"], _viewModel.Ui["Cancel"]);
                confirmation.Owner = this;
                if (confirmation.ShowDialog() != true)
                    return;
                if (MessageBox.Show(_viewModel.Ui["ConfirmAll"], _viewModel.Ui["ProductName"], MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    ShowResult(_viewModel.ApplyAllCapabilities());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, _viewModel.Ui["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RestoreLatestButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(_viewModel.Ui["RestoreQuestion"], _viewModel.Ui["ProductName"], MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            ShowResult(_viewModel.RestoreLatestCapability());
        }

        private void ApplyModeButton_Click(object sender, RoutedEventArgs e)
        {
            OperationResult result = _viewModel.ApplyCurrentMode();
            if (!result.Succeeded)
            {
                ShowResult(result);
                return;
            }

            ModeConfirmationWindow confirmation = new ModeConfirmationWindow(_viewModel.Ui["KeepModePrompt"], _viewModel.Ui["KeepMode"], _viewModel.Ui["RevertMode"], _viewModel.Ui["SecondsRemaining"], 15);
            confirmation.Owner = this;
            bool keep = confirmation.ShowDialog() == true;
            ShowResult(keep ? _viewModel.ConfirmCurrentMode() : _viewModel.RestoreCurrentMode());
        }

        private void ApplyScaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format(_viewModel.Ui["ConfirmScale"], _viewModel.SelectedScalePercent), _viewModel.Ui["ProductName"], MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            ShowResult(_viewModel.ApplyExperimentalScale());
        }

        private void RestoreScaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(_viewModel.Ui["RestoreScaleQuestion"], _viewModel.Ui["ProductName"], MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            ShowResult(_viewModel.RestoreLatestExperimentalScale());
        }

        private void DisplaySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("ms-settings:display");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, _viewModel.Ui["DisplaySettings"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportDiagnosticButton_Click(object sender, RoutedEventArgs e)
        {
            DiagnosticExportResult result = _viewModel.ExportDiagnosticPackage();
            string message = result.Succeeded
                ? _viewModel.Ui["DiagnosticExported"] + Environment.NewLine + result.ArchivePath
                : _viewModel.Ui["DiagnosticExportFailed"];
            MessageBox.Show(message, _viewModel.Ui["OperationResult"], MessageBoxButton.OK,
                result.Succeeded ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void OpenRecoveryFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", AppPaths.DataRoot);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, _viewModel.Ui["Recovery"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowResult(OperationResult result)
        {
            String message = result.Message;
            if (result.RestartRequired)
                message = _viewModel.Ui["RestartRequiredNotice"] + Environment.NewLine + Environment.NewLine + message;
            MessageBox.Show(message, _viewModel.Ui["OperationResult"], MessageBoxButton.OK, result.Succeeded ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }
}
