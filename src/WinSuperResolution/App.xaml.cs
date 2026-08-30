using System;
using System.Windows;
using System.Windows.Threading;
using WinSuperResolution.Services;

namespace WinSuperResolution
{
    public partial class App : Application
    {
        private readonly DiagnosticsService _diagnostics = new DiagnosticsService();

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _diagnostics.Write("Unhandled WPF exception: " + e.Exception);
            MessageBox.Show("An unexpected error was recorded in the portable logs folder. No additional changes will be applied.", "WinSuperResolution", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _diagnostics.Write("Unhandled application exception (terminating=" + e.IsTerminating + "): " + e.ExceptionObject);
        }
    }
}
