using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WinSuperResolution.Windows
{
    internal sealed class ModeConfirmationWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly TextBlock _countdown;
        private int _secondsRemaining;

        internal ModeConfirmationWindow(string prompt, string keepText, string revertText, string countdownTemplate, int seconds)
        {
            _secondsRemaining = seconds < 15 ? 15 : seconds;
            Title = "WinSuperResolution";
            Width = 430;
            Height = 190;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Grid grid = new Grid { Margin = new Thickness(18) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock { Text = prompt, TextWrapping = TextWrapping.Wrap });
            _countdown = new TextBlock { Margin = new Thickness(0, 12, 0, 0), FontWeight = FontWeights.SemiBold };
            Grid.SetRow(_countdown, 1);
            grid.Children.Add(_countdown);
            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button keep = new Button { Content = keepText, MinWidth = 128, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            keep.Click += delegate { DialogResult = true; Close(); };
            Button revert = new Button { Content = revertText, MinWidth = 104, IsCancel = true };
            revert.Click += delegate { DialogResult = false; Close(); };
            buttons.Children.Add(keep);
            buttons.Children.Add(revert);
            Grid.SetRow(buttons, 3);
            grid.Children.Add(buttons);
            Content = grid;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            Tag = countdownTemplate;
            Closed += delegate { _timer.Stop(); };
            UpdateCountdown();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _secondsRemaining--;
            if (_secondsRemaining <= 0)
            {
                DialogResult = false;
                Close();
                return;
            }
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            _countdown.Text = string.Format((string)Tag, _secondsRemaining);
        }
    }
}
