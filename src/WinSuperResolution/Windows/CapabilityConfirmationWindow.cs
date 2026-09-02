using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WinSuperResolution.Models;

namespace WinSuperResolution.Windows
{
    internal sealed class CapabilityConfirmationWindow : Window
    {
        internal CapabilityConfirmationWindow(string title, string prompt, IList<ResolutionPlan> plans, string applyText, string cancelText, string planSummaryTemplate, string activeSignalBasis, string registeredSurfaceBasis, string recoveryPolicy)
        {
            Title = title;
            Width = 760;
            Height = 560;
            MinWidth = 620;
            MinHeight = 420;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Grid grid = new Grid { Margin = new Thickness(18) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock { Text = prompt, TextWrapping = TextWrapping.Wrap });

            ListBox planList = new ListBox { Margin = new Thickness(0, 14, 0, 14) };
            foreach (ResolutionPlan plan in plans)
                planList.Items.Add(DescribePlan(plan, planSummaryTemplate, activeSignalBasis, registeredSurfaceBasis, recoveryPolicy));
            Grid.SetRow(planList, 1);
            grid.Children.Add(planList);

            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button cancel = new Button { Content = cancelText, MinWidth = 100, IsCancel = true, Margin = new Thickness(0, 0, 8, 0) };
            cancel.Click += delegate { DialogResult = false; Close(); };
            Button apply = new Button { Content = applyText, MinWidth = 120, IsDefault = true };
            apply.Click += delegate { DialogResult = true; Close(); };
            buttons.Children.Add(cancel);
            buttons.Children.Add(apply);
            Grid.SetRow(buttons, 2);
            grid.Children.Add(buttons);
            Content = grid;
        }

        private static string DescribePlan(ResolutionPlan plan, string planSummaryTemplate, string activeSignalBasis, string registeredSurfaceBasis, string recoveryPolicy)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(plan.Record.DisplayIdentity + " | " + plan.Record.ConnectionStatusText + " / " + plan.Record.MatchStatusText);
            string basis = plan.Basis == CalculationBasis.ActiveSize ? activeSignalBasis : registeredSurfaceBasis;
            builder.AppendLine(string.Format(planSummaryTemplate, plan.Magnification, basis, plan.BaseWidth, plan.BaseHeight, plan.TargetWidth, plan.TargetHeight, plan.Mutations.Count));
            builder.AppendLine(recoveryPolicy);
            foreach (RegistryMutation mutation in plan.Mutations)
                builder.AppendLine("HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\\" + mutation.RelativePath + " | " + mutation.OriginalWidth + " x " + mutation.OriginalHeight + " -> " + mutation.TargetWidth + " x " + mutation.TargetHeight);
            return builder.ToString();
        }
    }
}
