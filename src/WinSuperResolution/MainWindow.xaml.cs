using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;
using WinSuperResolution.Models;
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
            Loaded += MainWindow_Loaded;
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
            ShowResult(_viewModel.ApplyExperimentalScale());
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

        private void CopyDiagnosticButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_viewModel.BuildDiagnosticSummary());
            _viewModel.StatusText = _viewModel.Ui["DiagnosticCopied"];
        }

        private void ShowResult(OperationResult result)
        {
            MessageBox.Show(result.Message, _viewModel.Ui["OperationResult"], MessageBoxButton.OK, result.Succeeded ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }
}
