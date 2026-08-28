using System;
using WinSuperResolution.Models;
using WinSuperResolution.Resources;
using WinSuperResolution.Services;

namespace WinSuperResolution.SmokeTests
{
    internal static class Program
    {
        [STAThread]
        private static int Main()
        {
            try
            {
                TestActiveSignalPlan();
                TestPrimarySurfaceFallbackPlan();
                TestInvalidMagnification();
                TestEmbeddedLocalization();
                TestLiveDisplayEnumeration();
                TestExperimentalScaleSafetyGate();
                TestWindowMarkupLoads();
                Console.WriteLine("WinSuperResolution smoke tests passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static void TestActiveSignalPlan()
        {
            DisplayConfigurationRecord record = CreateRecord(1920, 1080, 2560, 1440);
            ResolutionPlan plan = new ResolutionPlanService().Build(record, 150);
            Assert(plan.Basis == CalculationBasis.ActiveSize, "ActiveSize must be preferred.");
            Assert(plan.TargetWidth == 3840 && plan.TargetHeight == 2160, "150% ActiveSize calculation is incorrect.");
            Assert(plan.Mutations.Count == 1 && plan.Mutations[0].TargetWidth == 3840, "All targets must receive the calculated size.");
        }

        private static void TestPrimarySurfaceFallbackPlan()
        {
            DisplayConfigurationRecord record = CreateRecord(1920, 1080, 0, 0);
            ResolutionPlan plan = new ResolutionPlanService().Build(record, 120);
            Assert(plan.Basis == CalculationBasis.PrimSurfSize, "PrimSurfSize must be the fallback basis.");
            Assert(plan.TargetWidth == 2304 && plan.TargetHeight == 1296, "Fallback plan calculation is incorrect.");
        }

        private static void TestInvalidMagnification()
        {
            bool rejected = false;
            try
            {
                new ResolutionPlanService().Build(CreateRecord(1920, 1080, 0, 0), 155);
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }
            Assert(rejected, "A magnification outside 10% increments must be rejected.");
        }

        private static void TestEmbeddedLocalization()
        {
            Assert(Strings.ForCulture("en-US")["Refresh"] == "Refresh", "English embedded resource is missing.");
            Assert(Strings.ForCulture("zh-CN")["Refresh"] == "刷新", "Chinese embedded resource is missing.");
            Assert(Strings.ForCulture("ru-RU")["Refresh"] == "Обновить", "Russian embedded resource is missing.");
            Assert(Strings.ForCulture("unknown")["ProductName"] == "WinSuperResolution", "Unsupported cultures must fall back to English.");
        }

        private static void TestLiveDisplayEnumeration()
        {
            System.Collections.Generic.IList<LiveDisplayInfo> displays = new LiveDisplayService().Enumerate();
            Assert(displays != null, "Live display enumeration returned null.");
        }

        private static void TestExperimentalScaleSafetyGate()
        {
            ExperimentalScaleService service = new ExperimentalScaleService(new JournalService());
            OperationResult result = service.Apply(null, 150);
            Assert(!result.Succeeded, "Experimental scaling must refuse an unverified target.");
            Assert(service.GetAvailableScalePercentages(null).Count == 0, "Unknown displays must not expose scale options.");
        }

        private static void TestWindowMarkupLoads()
        {
            WinSuperResolution.MainWindow window = new WinSuperResolution.MainWindow();
            Assert(window.Content != null, "The WPF main-window markup did not initialize.");
        }

        private static DisplayConfigurationRecord CreateRecord(int primaryWidth, int primaryHeight, int activeWidth, int activeHeight)
        {
            DisplayConfigurationRecord record = new DisplayConfigurationRecord();
            record.ConfigurationKey = "TEST";
            record.PrimarySurfaceWidth = primaryWidth;
            record.PrimarySurfaceHeight = primaryHeight;
            record.ActiveSignalWidth = activeWidth;
            record.ActiveSignalHeight = activeHeight;
            record.RegistryTargets.Add(new RegistryTarget
            {
                RelativePath = "TEST\\00\\00",
                PrimarySurfaceWidth = primaryWidth,
                PrimarySurfaceHeight = primaryHeight,
                ActiveSignalWidth = activeWidth,
                ActiveSignalHeight = activeHeight
            });
            return record;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
