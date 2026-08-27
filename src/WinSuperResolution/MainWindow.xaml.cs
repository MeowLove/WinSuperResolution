using System;
using System.Diagnostics;
using System.Windows;
using WinSuperResolution.ViewModels;

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

        private void DisplaySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("ms-settings:display");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Display Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyDiagnosticButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_viewModel.BuildDiagnosticSummary());
            _viewModel.StatusText = "Diagnostic summary copied to the clipboard.";
        }
    }
}
