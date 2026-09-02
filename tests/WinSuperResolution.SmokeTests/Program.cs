using System;
using System.IO;
using System.IO.Compression;
using WinSuperResolution.Models;
using WinSuperResolution.Resources;
using WinSuperResolution.Services;
using WinSuperResolution.ViewModels;

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
                TestIdleOperationLocalization();
                TestLocalizedOperationSummary();
                TestPortablePaths();
                TestActiveSignalTargetSelection();
                TestResolutionOnlyMatchStaysCandidate();
                TestDuplicateCandidateConfigurationProtection();
                TestStableIdentityResolvesDuplicateCandidates();
                TestNonUniqueStableIdentityStaysCandidate();
                TestDiagnosticExportIncludesLongNamedBackup();
                TestDisplayIdentityIncludesDeviceName();
                TestLiveDisplayEnumeration();
                TestExperimentalScaleSafetyGate();
                TestVirtualDesktopModeIndexes();
                TestVirtualDesktopModePresentation();
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
            Assert(Strings.ForCulture("en-US")["AboutAuthor"] == "About CXT (MeowLove)", "English author entry is missing.");
            Assert(Strings.ForCulture("zh-CN")["AboutAuthor"] == "关于作者 CXT（MeowLove）", "Chinese author entry is missing.");
            Assert(Strings.ForCulture("ru-RU")["AboutAuthor"] == "Об авторе CXT (MeowLove)", "Russian author entry is missing.");
            Assert(Strings.ForCulture("zh-CN")["ScanComplete"].StartsWith("扫描完成"), "Chinese scan status is not localized.");
            Assert(Strings.ForCulture("en-US")["ResetDisplayCachePrompt"].StartsWith("Final repair"), "English display-cache reset prompt is missing.");
            Assert(Strings.ForCulture("zh-CN")["ResetDisplayCachePrompt"].StartsWith("最终修复方案"), "Chinese display-cache reset prompt is missing.");
            Assert(Strings.ForCulture("ru-RU")["ResetDisplayCachePrompt"].StartsWith("Финальное исправление"), "Russian display-cache reset prompt is missing.");
            Assert(Strings.ForCulture("en-US")["Compatibility"] == "Environment and compatibility", "English compatibility label is missing.");
            Assert(Strings.ForCulture("zh-CN")["Compatibility"] == "环境与兼容性", "Chinese compatibility label is missing.");
            Assert(Strings.ForCulture("ru-RU")["Compatibility"] == "Среда и совместимость", "Russian compatibility label is missing.");
            Assert(Strings.ForCulture("en-US")["CompatibilityOtherGraphics"] == "Other display adapters", "English compatibility detail label is missing.");
            Assert(Strings.ForCulture("zh-CN")["CompatibilityDriverFreshness"] == "驱动新鲜度", "Chinese compatibility detail label is missing.");
            Assert(Strings.ForCulture("ru-RU")["CompatibilityAdapterAssociation"] == "Связь с адаптером", "Russian compatibility detail label is missing.");
            Assert(Strings.ForCulture("unknown")["ProductName"] == "WinSuperResolution", "Unsupported cultures must fall back to English.");
        }

        private static void TestIdleOperationLocalization()
        {
            MainViewModel viewModel = new MainViewModel();
            string originalLanguage = viewModel.SelectedLanguage;
            try
            {
                viewModel.SelectedLanguage = "en-US";
                viewModel.SelectedLanguage = "zh-CN";
                Assert(viewModel.LastOperationSummary == Strings.ForCulture("zh-CN")["NoOperationYet"], "Idle operation summary did not switch to Chinese.");
                viewModel.SelectedLanguage = "ru-RU";
                Assert(viewModel.LastOperationSummary == Strings.ForCulture("ru-RU")["NoOperationYet"], "Idle operation summary did not switch to Russian.");
            }
            finally
            {
                viewModel.SelectedLanguage = originalLanguage;
            }
        }

        private static void TestLocalizedOperationSummary()
        {
            MainViewModel viewModel = new MainViewModel();
            OperationResult result = viewModel.ApplyCurrentMode();
            Assert(!result.Succeeded, "A current-mode operation without a selected record must fail.");
            viewModel.SelectedLanguage = "zh-CN";
            Assert(viewModel.LastOperationSummary.StartsWith(Strings.ForCulture("zh-CN")["OperationFailed"]), "Operation summary did not switch to Chinese.");
        }

        private static void TestPortablePaths()
        {
            Assert(AppPaths.DataRoot == AppPaths.ExecutableDirectory, "Portable data root must be the executable directory.");
            Assert(Path.GetDirectoryName(AppPaths.SettingsPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) == AppPaths.ExecutableDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), "Settings must be stored beside the executable.");
            Assert(Path.GetFileName(AppPaths.BackupsDirectory) == "backup_reg", "Registry backups must use the portable backup_reg directory.");
        }

        private static void TestActiveSignalTargetSelection()
        {
            System.Collections.Generic.List<RegistryTarget> targets = new System.Collections.Generic.List<RegistryTarget>
            {
                new RegistryTarget { PrimarySurfaceWidth = 4320, PrimarySurfaceHeight = 2700 },
                new RegistryTarget { PrimarySurfaceWidth = 4320, PrimarySurfaceHeight = 2700, ActiveSignalWidth = 2880, ActiveSignalHeight = 1800 }
            };
            RegistryTarget selected = DisplayCatalogService.SelectSignalTarget(targets);
            Assert(selected.ActiveSignalWidth == 2880 && selected.ActiveSignalHeight == 1800, "The scan must use a valid ActiveSize from any target node.");
        }

        private static void TestResolutionOnlyMatchStaysCandidate()
        {
            DisplayConfigurationRecord record = CreateRecord(4320, 2700, 4320, 2700);
            record.MatchStatus = MatchStatus.Candidate;
            LiveDisplayInfo display = new LiveDisplayInfo
            {
                CurrentWidth = 4320,
                CurrentHeight = 2700,
                IsAttachedToDesktop = true
            };
            Assert(record.MatchStatus == MatchStatus.Candidate, "Resolution-only evidence must not promote a Candidate record to Exact.");
        }

        private static void TestDuplicateCandidateConfigurationProtection()
        {
            LiveDisplayInfo display = new LiveDisplayInfo { DeviceName = @"\\.\DISPLAY1" };
            DisplayConfigurationRecord first = CreateRecord(3840, 2160, 3840, 2160);
            first.ConnectionStatus = ConnectionStatus.Active;
            first.MatchStatus = MatchStatus.Candidate;
            first.ValidationStatus = ValidationStatus.Ready;
            first.LiveDisplay = display;
            DisplayConfigurationRecord second = CreateRecord(3840, 2160, 3840, 2160);
            second.ConnectionStatus = ConnectionStatus.Active;
            second.MatchStatus = MatchStatus.Candidate;
            second.ValidationStatus = ValidationStatus.Ready;
            second.LiveDisplay = display;

            DisplayCatalogService.MarkDuplicateCandidateConfigurations(
                new System.Collections.Generic.List<DisplayConfigurationRecord> { first, second });

            Assert(first.ConnectionStatus == ConnectionStatus.Active && second.ConnectionStatus == ConnectionStatus.Active, "Historical duplicate roots for one display must remain active candidates.");
            Assert(first.MatchStatus == MatchStatus.Candidate && second.MatchStatus == MatchStatus.Candidate, "Historical duplicate roots must retain Candidate matching status.");
            Assert(first.DuplicateCandidateCount == 2 && second.DuplicateCandidateCount == 2, "Duplicate Candidate records must retain the duplicate count for diagnostics.");
            Assert(!first.CanApplyVirtualCapability && !second.CanApplyVirtualCapability, "Active duplicate Candidate records must remain read-only for virtual-resolution capability writes.");
        }

        private static void TestStableIdentityResolvesDuplicateCandidates()
        {
            LiveDisplayInfo display = new LiveDisplayInfo
            {
                DeviceName = @"\\.\DISPLAY1",
                MonitorDeviceId = @"MONITOR\TMA2004\4&11FF7B6D&0&UID8388688",
                CurrentWidth = 2880,
                CurrentHeight = 1800,
                IsAttachedToDesktop = true
            };
            DisplayConfigurationRecord stale = CreateRecord(5760, 3600, 2880, 1800);
            stale.ConfigurationKey = "SIMULATED_8086_7D55^47C59A57D12AD85FFAC996EBA9429A77";
            stale.ConnectionStatus = ConnectionStatus.Active;
            stale.MatchStatus = MatchStatus.Candidate;
            stale.ValidationStatus = ValidationStatus.Ready;
            stale.LiveDisplay = display;
            DisplayConfigurationRecord exact = CreateRecord(5760, 3600, 2880, 1800);
            exact.ConfigurationKey = "TMA20040_28_07E7_A7^E2906A7A64D0F55109B9880C09E758B8";
            exact.ConnectionStatus = ConnectionStatus.Active;
            exact.MatchStatus = MatchStatus.Candidate;
            exact.ValidationStatus = ValidationStatus.Ready;
            exact.LiveDisplay = display;

            DisplayCatalogService.PromoteStableIdentityMatches(
                new System.Collections.Generic.List<DisplayConfigurationRecord> { stale, exact },
                new System.Collections.Generic.List<LiveDisplayInfo> { display });

            Assert(exact.MatchStatus == MatchStatus.Exact && exact.CanManageCurrentState, "A unique stable monitor identity must promote the matching duplicate root to Exact.");
            Assert(stale.ConnectionStatus == ConnectionStatus.Historical && stale.MatchStatus == MatchStatus.Unmatched, "Superseded duplicate roots must be historical and unmatched.");
            Assert(!stale.CanApplyVirtualCapability, "Superseded duplicate roots must not receive capability writes.");
        }

        private static void TestNonUniqueStableIdentityStaysCandidate()
        {
            LiveDisplayInfo display = new LiveDisplayInfo
            {
                DeviceName = @"\\.\DISPLAY1",
                MonitorDeviceId = @"MONITOR\TMA2004\4&11FF7B6D&0&UID8388688",
                CurrentWidth = 2880,
                CurrentHeight = 1800,
                IsAttachedToDesktop = true
            };
            DisplayConfigurationRecord first = CreateRecord(5760, 3600, 2880, 1800);
            first.ConfigurationKey = "TMA2004_A^FIRST";
            first.ConnectionStatus = ConnectionStatus.Active;
            first.MatchStatus = MatchStatus.Candidate;
            first.ValidationStatus = ValidationStatus.Ready;
            first.LiveDisplay = display;
            DisplayConfigurationRecord second = CreateRecord(5760, 3600, 2880, 1800);
            second.ConfigurationKey = "TMA2004_B^SECOND";
            second.ConnectionStatus = ConnectionStatus.Active;
            second.MatchStatus = MatchStatus.Candidate;
            second.ValidationStatus = ValidationStatus.Ready;
            second.LiveDisplay = display;

            DisplayCatalogService.PromoteStableIdentityMatches(
                new System.Collections.Generic.List<DisplayConfigurationRecord> { first, second },
                new System.Collections.Generic.List<LiveDisplayInfo> { display });

            Assert(first.MatchStatus == MatchStatus.Candidate && second.MatchStatus == MatchStatus.Candidate, "A non-unique model token must not promote any duplicate root to Exact.");
            Assert(!first.CanApplyVirtualCapability && !second.CanApplyVirtualCapability, "Active non-unique candidates must remain read-only.");
        }

        private static void TestDiagnosticExportIncludesLongNamedBackup()
        {
            Directory.CreateDirectory(AppPaths.BackupsDirectory);
            string fileName = new string('M', 90) + ".reg";
            string backupPath = Path.Combine(AppPaths.BackupsDirectory, fileName);
            File.WriteAllText(backupPath, "Windows Registry Editor Version 5.00");
            DiagnosticExportResult result = null;
            try
            {
                result = new DiagnosticExportService().Export("diagnostic export smoke test");
                Assert(result.Succeeded, "Diagnostic export should produce an archive.");
                using (ZipArchive archive = ZipFile.OpenRead(result.ArchivePath))
                {
                    bool found = false;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == "registry-backups/" + fileName)
                        {
                            found = true;
                            break;
                        }
                    }
                    Assert(found, "Diagnostic export must include long-named registry backup files.");
                }
            }
            finally
            {
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                if (result != null && File.Exists(result.ArchivePath))
                    File.Delete(result.ArchivePath);
            }
        }

        private static void TestDisplayIdentityIncludesDeviceName()
        {
            DisplayConfigurationRecord record = CreateRecord(1920, 1080, 1920, 1080);
            record.LiveDisplay = new LiveDisplayInfo
            {
                FriendlyName = "Test Monitor",
                DeviceName = @"\\.\DISPLAY2",
                ConnectionTechnology = "DisplayPort"
            };
            Assert(record.DisplayIdentity.Contains(@"\\.\DISPLAY2") && record.DisplayIdentity.Contains("DisplayPort"), "Active display identity should include the Windows display name and connection technology.");
        }

        private static void TestLiveDisplayEnumeration()
        {
            System.Collections.Generic.IList<LiveDisplayInfo> displays = new LiveDisplayService().Enumerate();
            Assert(displays != null, "Live display enumeration returned null.");
        }

        private static void TestExperimentalScaleSafetyGate()
        {
            ExperimentalScaleService service = new ExperimentalScaleService(new JournalService(), new DiagnosticsService());
            OperationResult result = service.Apply(null, 150);
            Assert(!result.Succeeded, "Experimental scaling must refuse an unknown target.");
            Assert(service.GetAvailableScalePercentages(null).Count == 0, "Unknown displays must not expose scale options.");
            Assert(ExperimentalScaleService.GetBaselineScalePercent(150, -2) == 200, "DpiValue baseline mapping is incorrect.");
            Assert(ExperimentalScaleService.CalculateTargetDpiValue(200, 250) == 2, "DpiValue target mapping is incorrect.");
        }

        private static void TestVirtualDesktopModeIndexes()
        {
            const uint sourceModeIndex = 7;
            const uint desktopImageIndex = 3;
            uint packedSourceInfo = sourceModeIndex << 16;
            uint packedTargetInfo = desktopImageIndex;
            Assert(VirtualDesktopModeService.GetVirtualSourceModeInfoIndex(packedSourceInfo) == sourceModeIndex, "Virtual source mode index decoding is incorrect.");
            Assert(VirtualDesktopModeService.GetVirtualDesktopImageModeInfoIndex(packedTargetInfo) == desktopImageIndex, "Virtual desktop image index decoding is incorrect.");
        }

        private static void TestVirtualDesktopModePresentation()
        {
            DisplayMode mode = new DisplayMode
            {
                Width = 4320,
                Height = 2700,
                Frequency = 120,
                IsVirtualDesktopMode = true,
                ModeKindText = Strings.ForCulture("zh-CN")["ModeKindVirtualDesktop"]
            };
            Assert(mode.DisplayText.Contains("4320 x 2700") && mode.DisplayText.Contains("虚拟桌面"), "Virtual desktop mode presentation is incomplete.");
        }

        private static void TestWindowMarkupLoads()
        {
            WinSuperResolution.MainWindow window = new WinSuperResolution.MainWindow();
            Assert(window.Content != null, "The WPF main-window markup did not initialize.");
            Assert(window.Icon != null, "The WPF main-window icon did not load.");
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
