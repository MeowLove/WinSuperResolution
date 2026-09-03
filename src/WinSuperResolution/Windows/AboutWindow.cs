using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinSuperResolution.Resources;

namespace WinSuperResolution.Windows
{
    internal sealed class AboutWindow : Window
    {
        private const string WebsiteUrl = "https://www.cxthhhhh.com/";
        private const string IntroductionUrl = "https://www.cxthhhhh.com/2026/09/03/winsuperresolution-windows-hidpi-style-scaling-v3.html";
        private const string GitHubUrl = "https://github.com/MeowLove/WinSuperResolution";

        private static readonly Brush HeaderBrush = new SolidColorBrush(Color.FromRgb(20, 61, 67));
        private static readonly Brush AccentBrush = new SolidColorBrush(Color.FromRgb(8, 126, 139));
        private static readonly Brush LinkBrush = new SolidColorBrush(Color.FromRgb(232, 242, 244));
        private static readonly Brush LinkTextBrush = new SolidColorBrush(Color.FromRgb(18, 60, 70));
        private readonly LocalizedStrings _ui;

        internal AboutWindow(LocalizedStrings ui, string version)
        {
            _ui = ui;
            Title = ui["AboutTitle"];
            Width = 560;
            MinWidth = 500;
            MinHeight = 420;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(244, 246, 248));

            Grid layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border header = new Border { Background = HeaderBrush, Padding = new Thickness(24, 18, 24, 16) };
            StackPanel headerContent = new StackPanel();
            headerContent.Children.Add(new TextBlock { Text = ui["ProductName"], Foreground = Brushes.White, FontSize = 24, FontWeight = FontWeights.SemiBold });
            headerContent.Children.Add(new TextBlock { Text = ui["AboutDescription"], Foreground = new SolidColorBrush(Color.FromRgb(200, 219, 222)), FontSize = 13, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 0, 11) });
            Border versionBadge = new Border { Background = AccentBrush, CornerRadius = new CornerRadius(4), Padding = new Thickness(9, 4, 9, 4), HorizontalAlignment = HorizontalAlignment.Left };
            versionBadge.Child = new TextBlock { Text = ui["AboutVersion"] + "  " + version, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold };
            headerContent.Children.Add(versionBadge);
            header.Child = headerContent;
            layout.Children.Add(header);

            Border content = new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(216, 224, 230)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(7), Margin = new Thickness(20, 16, 20, 12), Padding = new Thickness(18, 16, 18, 12) };
            StackPanel contentStack = new StackPanel();
            contentStack.Children.Add(new TextBlock { Text = ui["AboutAuthorName"], Foreground = new SolidColorBrush(Color.FromRgb(23, 33, 43)), FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) });
            contentStack.Children.Add(CreateLinkButton(ui["AboutWebsite"], WebsiteUrl));
            contentStack.Children.Add(CreateLinkButton(ui["AboutIntroduction"], IntroductionUrl));
            contentStack.Children.Add(CreateLinkButton(ui["AboutGitHub"], GitHubUrl));
            content.Child = contentStack;
            Grid.SetRow(content, 1);
            layout.Children.Add(content);

            StackPanel footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20, 0, 20, 18) };
            Button close = CreateButton(ui["Close"], new SolidColorBrush(Color.FromRgb(93, 107, 120)), Brushes.White, 104, new Thickness(0));
            close.IsCancel = true;
            close.Click += delegate { Close(); };
            footer.Children.Add(close);
            Grid.SetRow(footer, 2);
            layout.Children.Add(footer);
            Content = layout;
        }

        private Button CreateLinkButton(string text, string url)
        {
            Button button = CreateButton(text, LinkBrush, LinkTextBrush, 0, new Thickness(0, 0, 0, 8));
            button.Height = 34;
            button.Margin = new Thickness(0, 0, 0, 6);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Click += delegate { OpenLink(url); };
            return button;
        }

        private static Button CreateButton(string text, Brush background, Brush foreground, double minWidth, Thickness margin)
        {
            Button button = new Button { Content = text, Background = background, Foreground = foreground, MinWidth = minWidth, Height = 38, Margin = margin, FontWeight = FontWeights.SemiBold, Cursor = System.Windows.Input.Cursors.Hand };
            button.Template = CreateButtonTemplate(text, background, foreground);
            return button;
        }

        private static ControlTemplate CreateButtonTemplate(string text, Brush background, Brush foreground)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, background);
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            border.SetValue(Border.PaddingProperty, new Thickness(14, 0, 14, 0));
            FrameworkElementFactory content = new FrameworkElementFactory(typeof(TextBlock));
            content.SetValue(TextBlock.TextProperty, text);
            content.SetValue(TextBlock.ForegroundProperty, foreground);
            content.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            content.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            content.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            return template;
        }

        private void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show(_ui["AboutLinkOpenFailed"], Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
