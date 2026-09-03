using System;
using System.Collections.Generic;

namespace WinSuperResolution.Resources
{
    public sealed class LocalizedStrings
    {
        private readonly string _culture;

        internal LocalizedStrings(string culture)
        {
            _culture = Strings.IsSupported(culture) ? culture : Strings.DefaultCulture;
        }

        public string this[string key]
        {
            get { return Strings.Get(_culture, key); }
        }
    }

    internal static class Strings
    {
        internal const string DefaultCulture = "en-US";
        internal static readonly IList<string> SupportedCultures = new List<string> { "en-US", "zh-CN", "ru-RU" };

        private static readonly IDictionary<string, IDictionary<string, string>> Values = CreateValues();

        private static IDictionary<string, IDictionary<string, string>> CreateValues()
        {
            IDictionary<string, string> english = CreateEnglish();
            english.Add("HomeTab", "1. Home");
            english.Add("HomeTabDescription", "Check compatibility and recover");
            english.Add("SuperResolutionTab", "2. Super Resolution");
            english.Add("SuperResolutionTabDescription", "Write virtual-resolution capability");
            english.Add("ScalingTab", "3. Scaling");
            english.Add("ScalingTabDescription", "Set resolution and scaling after restart");
            english.Add("RecommendedSequence", "Recommended sequence");
            english.Add("SuperResolutionWorkflow", "Apply the super-resolution plan, then restart Windows immediately. If the new modes are still missing after restart, select Refresh. Next, continue in 3. Scaling or use Display Settings at the top to choose resolution and scaling in Windows. Choose one method; do not repeat the same setting in both places.");
            english.Add("SuperResolutionTroubleshooting", "New modes missing or display state still abnormal?");
            english.Add("SuperResolutionTroubleshootingDetails", "First confirm that Windows has restarted, Refresh has been selected, and driver-level DSR or dynamic resolution is disabled. If the problem persists, use Clear display cache (final repair) at the top. It backs up and clears Windows display caches, then restarts the computer immediately; export a diagnostic package first.");
            english.Add("ScalingWorkflow", "After new modes become available, set resolution and experimental scaling here, or use Display Settings at the top to complete the equivalent Windows setting. Choose the method that fits the active display; you do not need to set the same value twice.");
            english["AboutAuthor"] = "About";
            AddCompatibilityStrings(english, "Environment and compatibility", "Unsupported", "Experimental", "Can try", "System", "Processor", "Active graphics", "Display path", "View details", "Select an active display to inspect its virtual desktop path.", "Windows did not report virtual desktop support for this active display path.", "The active display path supports virtual desktop modes, but its matched graphics driver is old or its date is unavailable.", "The active display path and its matched graphics driver meet the advisory checks.");
            english.Add("CompatibilityPathSupported", "Virtual desktop path: supported for {0}.");
            english.Add("CompatibilityPathUnavailable", "Virtual desktop path: Windows could not confirm support for {0}.");
            AddCompatibilityDetailStrings(english, "Selected display", "Adapter association", "Driver freshness", "Other detected adapters", "Select an active display.", "Exact name match with the Windows active display adapter.", "Windows reported an active display adapter, but its WMI driver entry could not be matched.", "Driver date unavailable.", "Age: {0} days; advisory limit: {1} days.", "No other display adapters detected.");
            english.Add("CompatibilityGpuControlPanels", "GPU control panel");
            english.Add("CompatibilityGpuControlPanelInstalled", "Installed");
            english.Add("CompatibilityGpuControlPanelNotInstalled", "Not installed");
            english.Add("CompatibilityNoGpuControlPanels", "None detected");
            english.Add("CompatibilityGpuControlPanelWarning", "{0} detected. Its driver-level DSR or dynamic-resolution setting may override this program; disable that feature before testing.");
            english.Add("CompatibilityGpuControlPanelNoWarning", "No known NVIDIA, AMD, or Intel GPU control panel was detected.");
            english.Add("CompatibilityScaleWarning", "Windows is currently using {0}% scaling. After a virtual-resolution change, Windows may need a matching scale adjustment; if the interface size or usable area looks wrong, review Display Settings or restore the capability.");
            AddAboutStrings(english, "About WinSuperResolution", "Higher Virtual Resolution & HiDPI-Style Scaling for Windows", "Version", "Created by CXT (MeowLove)", "Official website", "Introduction", "GitHub", "Close", "The selected link could not be opened.");
            AddOperationStrings(english, "Virtual-resolution capability was written and verified. Restart Windows or the display driver before expecting new modes.", "Virtual-resolution capability was not applied. Check the diagnostic package for technical details.", "The latest virtual-resolution capability journal was restored.", "The latest virtual-resolution capability journal could not be restored. Check the diagnostic package for technical details.", "Desktop mode applied. Confirm it in the countdown window to retain it.", "Desktop mode was not applied. Check the diagnostic package for technical details.", "The new desktop mode was retained.", "The new desktop mode could not be retained.", "The original desktop mode was restored.", "The original desktop mode could not be restored.", "Experimental scale was not applied. Check the diagnostic package for technical details.", "Windows display cache reset was not completed. Check the diagnostic package for technical details.", "The operation was not completed. Check the diagnostic package for technical details.", "{0}% using {1}: {2} x {3} -> {4} x {5}; {6} registry target(s).", "active signal", "registered surface", "The capability preview could not be generated.", "Windows Display Settings could not be opened.", "The author page could not be opened.", "The recovery folder could not be opened.");
            english.Add("CapabilityRecoveryPolicy", "Failure policy: automatically attempt reverse-order journal recovery.");
            english.Add("UnexpectedError", "An unexpected error was recorded in the portable logs folder. No additional changes will be applied.");
            IDictionary<string, string> chinese = CreateChinese();
            chinese.Add("HomeTab", "1. 首页");
            chinese.Add("HomeTabDescription", "环境检查与恢复");
            chinese.Add("SuperResolutionTab", "2. 超分");
            chinese.Add("SuperResolutionTabDescription", "写入虚拟分辨率能力");
            chinese.Add("ScalingTab", "3. 缩放");
            chinese.Add("ScalingTabDescription", "重启后设置分辨率与缩放");
            chinese.Add("RecommendedSequence", "推荐操作顺序");
            chinese.Add("SuperResolutionWorkflow", "应用超分分辨率计划后，请立即重启 Windows。重启后如未看到新模式，请先点击“刷新”。随后可前往“3. 缩放”继续设置，也可点击顶部“显示设置”，在 Windows 中手动选择分辨率和缩放；两种方式任选其一，无需重复设置。");
            chinese.Add("SuperResolutionTroubleshooting", "新模式未出现或显示状态持续异常？");
            chinese.Add("SuperResolutionTroubleshootingDetails", "请先确认 Windows 已重启、已点击“刷新”，且显卡控制面板中的 DSR 或动态分辨率已关闭。仍无法恢复时，可使用顶部“清理显示缓存（最终修复）”。该操作会备份并清除 Windows 显示缓存，然后立即重启电脑；建议先导出诊断包。");
            chinese.Add("ScalingWorkflow", "新模式可用后，可在本页设置分辨率和实验性缩放，也可点击顶部“显示设置”交由 Windows 完成相应设置。请按当前显示器实际情况选择一种方式，无需在两处重复设置相同的值。");
            chinese["AboutAuthor"] = "关于";
            AddCompatibilityStrings(chinese, "环境与兼容性", "不支持", "实验性", "可尝试", "系统", "处理器", "当前显示适配器", "显示路径", "查看详细依据", "请选择活动显示器以检查其虚拟桌面路径。", "Windows 未报告此活动显示路径支持虚拟桌面模式。", "活动显示路径支持虚拟桌面模式，但与其匹配的显卡驱动过旧或无法取得驱动日期。", "活动显示路径及其匹配的显卡驱动均通过了提示性检查。");
            chinese.Add("CompatibilityPathSupported", "虚拟桌面路径：{0} 已报告支持。");
            chinese.Add("CompatibilityPathUnavailable", "虚拟桌面路径：Windows 无法确认 {0} 支持该功能。");
            AddCompatibilityDetailStrings(chinese, "所选显示器", "适配器关联", "驱动新鲜度", "其它已检测适配器", "请选择活动显示器。", "与 Windows 当前显示适配器名称精确匹配。", "Windows 已报告当前显示适配器，但无法在 WMI 驱动清单中精确匹配。", "无法取得驱动日期。", "距驱动日期：{0} 天；提示上限：{1} 天。", "未检测到其它显示适配器。");
            chinese.Add("CompatibilityGpuControlPanels", "显卡控制面板");
            chinese.Add("CompatibilityGpuControlPanelInstalled", "已安装");
            chinese.Add("CompatibilityGpuControlPanelNotInstalled", "未安装");
            chinese.Add("CompatibilityNoGpuControlPanels", "未检测到");
            chinese.Add("CompatibilityGpuControlPanelWarning", "检测到 {0}。其驱动级 DSR 或动态分辨率设置可能覆盖本程序；测试前请关闭相关功能。");
            chinese.Add("CompatibilityGpuControlPanelNoWarning", "未检测到已知的 NVIDIA、AMD 或 Intel 显卡控制面板。");
            chinese.Add("CompatibilityScaleWarning", "Windows 当前缩放为 {0}%。应用虚拟分辨率变化后，Windows 可能需要匹配的缩放调整；如果界面大小或可用区域异常，请检查显示设置，或恢复虚拟分辨率能力。");
            AddAboutStrings(chinese, "关于 WinSuperResolution", "适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验", "版本", "作者：CXT（MeowLove）", "官网", "介绍页面", "GitHub", "关闭", "无法打开所选链接。");
            AddOperationStrings(chinese, "虚拟分辨率能力已写入并验证。请重启 Windows 或重新初始化显示驱动后再等待新模式出现。", "虚拟分辨率能力未应用。请检查诊断包中的技术详情。", "最近一次虚拟分辨率能力 Journal 已恢复。", "无法恢复最近一次虚拟分辨率能力 Journal。请检查诊断包中的技术详情。", "桌面分辨率已应用。请在倒计时窗口中确认保留。", "桌面分辨率未应用。请检查诊断包中的技术详情。", "新的桌面分辨率已保留。", "无法保留新的桌面分辨率。", "原始桌面分辨率已恢复。", "无法恢复原始桌面分辨率。", "实验性缩放未应用。请检查诊断包中的技术详情。", "Windows 显示缓存清理未完成。请检查诊断包中的技术详情。", "操作未完成。请检查诊断包中的技术详情。", "{0}% 使用{1}：{2} x {3} -> {4} x {5}；{6} 个注册表目标。", "活动信号", "已注册表面", "无法生成能力预览。", "无法打开 Windows 显示设置。", "无法打开作者页面。", "无法打开恢复目录。");
            chinese.Add("CapabilityRecoveryPolicy", "失败策略：自动按相反顺序尝试 Journal 恢复。");
            chinese.Add("UnexpectedError", "发生意外错误，已记录到便携日志目录。不会继续执行其他修改。" );
            IDictionary<string, string> russian = CreateRussian();
            russian.Add("HomeTab", "1. Главная");
            russian.Add("HomeTabDescription", "Проверка совместимости и восстановление");
            russian.Add("SuperResolutionTab", "2. Сверхразрешение");
            russian.Add("SuperResolutionTabDescription", "Запись возможностей виртуального разрешения");
            russian.Add("ScalingTab", "3. Масштабирование");
            russian.Add("ScalingTabDescription", "Настройка разрешения и масштаба после перезагрузки");
            russian.Add("RecommendedSequence", "Рекомендуемый порядок");
            russian.Add("SuperResolutionWorkflow", "Примените план сверхразрешения и сразу перезагрузите Windows. Если новые режимы не появились после перезагрузки, нажмите «Обновить». Затем перейдите к 3. Масштабирование или откройте сверху Параметры дисплея, чтобы выбрать разрешение и масштаб в Windows. Выберите один способ и не задавайте один параметр в обоих местах.");
            russian.Add("SuperResolutionTroubleshooting", "Новые режимы не появились или состояние дисплея остаётся некорректным?");
            russian.Add("SuperResolutionTroubleshootingDetails", "Сначала убедитесь, что Windows перезагружена, нажато «Обновить», а DSR или динамическое разрешение в панели управления видеокартой отключено. Если проблема сохраняется, используйте сверху «Очистить кэш дисплея (последнее средство)». Операция создаст резервную копию, очистит кэш дисплея Windows и сразу перезагрузит компьютер; сначала экспортируйте диагностический пакет.");
            russian.Add("ScalingWorkflow", "Когда новые режимы станут доступны, настройте здесь разрешение и экспериментальное масштабирование либо используйте сверху Параметры дисплея для соответствующей настройки Windows. Выберите способ для активного дисплея; не нужно дважды задавать одно значение.");
            russian["AboutAuthor"] = "О программе";
            AddCompatibilityStrings(russian, "Среда и совместимость", "Не поддерживается", "Экспериментально", "Можно попробовать", "Система", "Процессор", "Активный графический адаптер", "Путь дисплея", "Показать подробности", "Выберите активный дисплей для проверки его пути виртуального рабочего стола.", "Windows не сообщает о поддержке виртуального рабочего стола для этого активного пути дисплея.", "Активный путь дисплея поддерживает виртуальный рабочий стол, но драйвер соответствующего графического адаптера устарел или его дата недоступна.", "Активный путь дисплея и соответствующий графический драйвер проходят рекомендательные проверки.");
            russian.Add("CompatibilityPathSupported", "Путь виртуального рабочего стола: поддерживается для {0}.");
            russian.Add("CompatibilityPathUnavailable", "Путь виртуального рабочего стола: Windows не может подтвердить поддержку для {0}.");
            AddCompatibilityDetailStrings(russian, "Выбранный дисплей", "Связь с адаптером", "Свежесть драйвера", "Другие обнаруженные адаптеры", "Выберите активный дисплей.", "Точное совпадение имени с активным адаптером дисплея Windows.", "Windows сообщает об активном адаптере, но его запись драйвера WMI не удалось сопоставить.", "Дата драйвера недоступна.", "Возраст: {0} дн.; рекомендательный предел: {1} дн.", "Другие адаптеры дисплея не обнаружены.");
            russian.Add("CompatibilityGpuControlPanels", "Панель управления GPU");
            russian.Add("CompatibilityGpuControlPanelInstalled", "Установлена");
            russian.Add("CompatibilityGpuControlPanelNotInstalled", "Не установлена");
            russian.Add("CompatibilityNoGpuControlPanels", "Не обнаружены");
            russian.Add("CompatibilityGpuControlPanelWarning", "Обнаружено: {0}. Функция DSR или динамическое разрешение на уровне драйвера может переопределить эту программу; отключите её перед тестированием.");
            russian.Add("CompatibilityGpuControlPanelNoWarning", "Известные панели NVIDIA, AMD или Intel не обнаружены.");
            russian.Add("CompatibilityScaleWarning", "Текущий масштаб Windows: {0}%. После изменения виртуального разрешения может потребоваться согласовать масштаб; если размер интерфейса или рабочая область выглядят неправильно, проверьте параметры дисплея или восстановите возможность виртуального разрешения.");
            AddAboutStrings(russian, "О WinSuperResolution", "Более высокое виртуальное разрешение и масштабирование в стиле HiDPI для Windows", "Версия", "Автор: CXT (MeowLove)", "Официальный сайт", "Страница с описанием", "GitHub", "Закрыть", "Не удалось открыть выбранную ссылку.");
            AddOperationStrings(russian, "Возможность виртуального разрешения записана и проверена. Перезапустите Windows или драйвер дисплея, прежде чем ожидать новые режимы.", "Возможность виртуального разрешения не применена. Проверьте технические сведения в пакете диагностики.", "Последний журнал возможности виртуального разрешения восстановлен.", "Не удалось восстановить последний журнал возможности виртуального разрешения. Проверьте технические сведения в пакете диагностики.", "Режим рабочего стола применён. Подтвердите его в окне обратного отсчёта, чтобы сохранить.", "Режим рабочего стола не применён. Проверьте технические сведения в пакете диагностики.", "Новый режим рабочего стола сохранён.", "Не удалось сохранить новый режим рабочего стола.", "Исходный режим рабочего стола восстановлен.", "Не удалось восстановить исходный режим рабочего стола.", "Экспериментальный масштаб не применён. Проверьте технические сведения в пакете диагностики.", "Сброс кэша дисплея Windows не завершён. Проверьте технические сведения в пакете диагностики.", "Операция не завершена. Проверьте технические сведения в пакете диагностики.", "{0}% с использованием {1}: {2} x {3} -> {4} x {5}; целей реестра: {6}.", "активный сигнал", "зарегистрированная поверхность", "Не удалось создать предварительный просмотр возможности.", "Не удалось открыть параметры дисплея Windows.", "Не удалось открыть страницу автора.", "Не удалось открыть папку восстановления.");
            russian.Add("CapabilityRecoveryPolicy", "Политика при ошибке: автоматически попытаться восстановить журнал в обратном порядке.");
            russian.Add("UnexpectedError", "Непредвиденная ошибка записана в папку переносимых журналов. Дополнительные изменения выполняться не будут.");
            return new Dictionary<string, IDictionary<string, string>>
            {
                { "en-US", english },
                { "zh-CN", chinese },
                { "ru-RU", russian }
            };
        }

        private static void AddCompatibilityStrings(IDictionary<string, string> dictionary, string title, string unsupported, string experimental, string canTry, string system, string processor, string graphics, string displayPath, string details, string noSelectionReason, string unsupportedReason, string experimentalReason, string canTryReason)
        {
            dictionary.Add("Compatibility", title);
            dictionary.Add("CompatibilityUnsupported", unsupported);
            dictionary.Add("CompatibilityExperimental", experimental);
            dictionary.Add("CompatibilityCanTry", canTry);
            dictionary.Add("CompatibilitySystem", system);
            dictionary.Add("CompatibilityProcessor", processor);
            dictionary.Add("CompatibilityGraphics", graphics);
            dictionary.Add("CompatibilityDisplayPath", displayPath);
            dictionary.Add("CompatibilityDetails", details);
            dictionary.Add("CompatibilityNoSelectionReason", noSelectionReason);
            dictionary.Add("CompatibilityUnsupportedReason", unsupportedReason);
            dictionary.Add("CompatibilityExperimentalReason", experimentalReason);
            dictionary.Add("CompatibilityCanTryReason", canTryReason);
        }

        private static void AddCompatibilityDetailStrings(IDictionary<string, string> dictionary, string selectedDisplay, string adapterAssociation, string driverFreshness, string otherGraphics, string noSelectedDisplay, string adapterMatched, string adapterNotMatched, string driverUnavailable, string driverAge, string noOtherGraphics)
        {
            dictionary.Add("CompatibilitySelectedDisplay", selectedDisplay);
            dictionary.Add("CompatibilityAdapterAssociation", adapterAssociation);
            dictionary.Add("CompatibilityDriverFreshness", driverFreshness);
            dictionary.Add("CompatibilityOtherGraphics", otherGraphics);
            dictionary.Add("CompatibilityNoSelectedDisplay", noSelectedDisplay);
            dictionary.Add("CompatibilityAdapterMatched", adapterMatched);
            dictionary.Add("CompatibilityAdapterNotMatched", adapterNotMatched);
            dictionary.Add("CompatibilityDriverUnavailable", driverUnavailable);
            dictionary.Add("CompatibilityDriverAge", driverAge);
            dictionary.Add("CompatibilityNoOtherGraphics", noOtherGraphics);
        }

        private static void AddAboutStrings(IDictionary<string, string> dictionary, string title, string description, string version, string author, string website, string introduction, string github, string close, string linkOpenFailed)
        {
            dictionary.Add("AboutTitle", title);
            dictionary.Add("AboutDescription", description);
            dictionary.Add("AboutVersion", version);
            dictionary.Add("AboutAuthorName", author);
            dictionary.Add("AboutWebsite", website);
            dictionary.Add("AboutIntroduction", introduction);
            dictionary.Add("AboutGitHub", github);
            dictionary.Add("Close", close);
            dictionary.Add("AboutLinkOpenFailed", linkOpenFailed);
        }

        private static void AddOperationStrings(IDictionary<string, string> dictionary, string capabilityApplied, string capabilityFailed, string capabilityRestored, string capabilityRestoreFailed, string modeApplied, string modeFailed, string modeRetained, string modeRetainFailed, string modeRestored, string modeRestoreFailed, string scaleFailed, string displayCacheFailed, string operationFailedDetail, string planSummary, string activeSignal, string registeredSurface, string capabilityPreviewFailed, string displaySettingsOpenFailed, string authorPageOpenFailed, string recoveryFolderOpenFailed)
        {
            dictionary.Add("CapabilityApplied", capabilityApplied);
            dictionary.Add("CapabilityFailed", capabilityFailed);
            dictionary.Add("CapabilityRestored", capabilityRestored);
            dictionary.Add("CapabilityRestoreFailed", capabilityRestoreFailed);
            dictionary.Add("ModeApplied", modeApplied);
            dictionary.Add("ModeFailed", modeFailed);
            dictionary.Add("ModeRetained", modeRetained);
            dictionary.Add("ModeRetainFailed", modeRetainFailed);
            dictionary.Add("ModeRestored", modeRestored);
            dictionary.Add("ModeRestoreFailed", modeRestoreFailed);
            dictionary.Add("ScaleFailed", scaleFailed);
            dictionary.Add("DisplayCacheFailedDetail", displayCacheFailed);
            dictionary.Add("OperationFailedDetail", operationFailedDetail);
            dictionary.Add("PlanSummary", planSummary);
            dictionary.Add("PlanBasisActiveSize", activeSignal);
            dictionary.Add("PlanBasisPrimSurfSize", registeredSurface);
            dictionary.Add("CapabilityPreviewFailed", capabilityPreviewFailed);
            dictionary.Add("DisplaySettingsOpenFailed", displaySettingsOpenFailed);
            dictionary.Add("AuthorPageOpenFailed", authorPageOpenFailed);
            dictionary.Add("RecoveryFolderOpenFailed", recoveryFolderOpenFailed);
        }

        internal static bool IsSupported(string culture)
        {
            return !string.IsNullOrEmpty(culture) && Values.ContainsKey(culture);
        }

        internal static LocalizedStrings ForCulture(string culture)
        {
            return new LocalizedStrings(culture);
        }

        internal static string Get(string culture, string key)
        {
            IDictionary<string, string> dictionary;
            string value;
            if (Values.TryGetValue(culture, out dictionary) && dictionary.TryGetValue(key, out value))
                return value;
            if (Values[DefaultCulture].TryGetValue(key, out value))
                return value;
            return key;
        }

        private static IDictionary<string, string> CreateEnglish()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "Higher Virtual Resolution & HiDPI-Style Scaling for Windows",
                "Refresh", "Refresh", "DisplaySettings", "Display Settings", "ExportDiagnostic", "Export Diagnostic Package", "DiagnosticExported", "Diagnostic package exported to: ", "DiagnosticExportFailed", "Diagnostic package could not be created.", "ResetDisplayCache", "Reset display cache (final repair)", "ResetDisplayCachePrompt", "Final repair: WinSuperResolution will back up and delete all Windows display caches (GraphicsDrivers\\Configuration, Connectivity, and ScaleFactors). Windows will rebuild them after an immediate restart. Save your work. Continue?", "DisplayCacheResetSuccess", "Windows display caches were cleared and backups were saved. Windows will restart now.", "DisplayCacheResetFailed", "The Windows display caches could not be cleared. No restart was requested.", "RestartFailed", "Windows could not be restarted automatically. Restart Windows manually.", "AboutAuthor", "About CXT (MeowLove)", "Language", "Language",
                "RegisteredConfigurations", "Registered display configurations", "Status", "Status", "Configuration", "Configuration", "Display", "Display", "Surface", "Surface", "Signal", "Signal", "Association", "Association",
                "CapabilityPlan", "Virtual-resolution capability plan", "Magnification", "Magnification:", "BuildPlan", "Build Plan", "ApplySelected", "Apply selected capability", "ApplyAll", "Apply all capabilities", "RestoreLatest", "Restore latest",
                "CurrentState", "Current Windows desktop mode and scale", "AvailableModes", "Available desktop and driver modes", "ApplyMode", "Test & apply desktop mode", "ExperimentalScale", "Experimental per-monitor scale", "ApplyScale", "Apply experimental scale", "ModeNoSelection", "Select a display record to inspect current Windows modes.", "ModeRequiresActive", "Current-mode changes require an Active display record.", "ModeRequiresConflict", "Current-mode changes are disabled because multiple registry configurations match this Windows display.", "ModeRequiresExact", "Current-mode changes are disabled because this display association is Candidate, not Exact.", "ModeNoLiveDisplay", "The active display has no live Windows display object.", "ModeNoModes", "No Windows display modes are available for this record.", "ModeAvailable", "Virtual desktop modes use the Windows display configuration path; driver-reported modes use the legacy driver path.", "ModeKindVirtualDesktop", "Virtual desktop", "ModeKindDriver", "Driver-reported",
                "ScaleUnavailable", "Direct scale control is guarded by a verified compatibility profile.", "CurrentStateGuard", "Current-mode changes require an Active + Exact association. Experimental scale uses an independent current-user PerMonitorSettings check; registry capability edits remain separate from the current Windows mode.",
                "CapabilityWarning", "Virtual-resolution capability is a registry compatibility ceiling, not the current Windows resolution or current display scale.", "KeepMode", "Keep this display mode", "RevertMode", "Revert now",
                "KeepModePrompt", "Keep the new display mode? It will be restored automatically when the countdown ends.", "ConfirmCapability", "Apply the prepared virtual-resolution capability plan? A full registry export and a value-level journal will be created first. Windows may need a restart or display-driver reinitialization before new modes appear.",
                "ConfirmAll", "Apply the prepared virtual-resolution capability plan to all eligible registered records? One backup and one journal will protect the batch.", "OperationResult", "Operation result", "Error", "Error", "Cancel", "Cancel", "RestoreQuestion", "Restore the latest virtual-resolution capability journal?", "SecondsRemaining", "{0} seconds remaining",
                "SelectConfiguration", "Select a registered configuration to inspect its virtual-resolution capability.", "SelectedSummary", "{0}\nTargets: {1}\nMatch: {2}; connection: {3}\n{4}\n{5}", "NoLiveDisplay", "No live display association is selected.", "CurrentStateSummary", "VirtualResolutionCapability: {0}; CurrentDisplayMode: {1}; CurrentPerMonitorScale: {2}.",
                "NoPlan", "No plan has been built.", "ScanComplete", "Scan complete: {0} configuration root(s), {1} writable target(s).", "ScanFailed", "Scan failed: ", "PreviewOnly", "Preview only; no registry values were changed.", "PlanBuilt", "Resolution plan built successfully.", "PlanUnavailable", "Plan unavailable: ", "PlanValidationFailed", "Plan validation failed.",
                "OperationSucceeded", "Operation completed.", "OperationFailed", "Operation was not completed.", "BackupPathLabel", "Registry backup: ", "JournalPathLabel", "Journal: ", "RestartRequiredNotice", "Restart required: save your work, then restart Windows or reinitialize the display driver before expecting new display modes.",
                "ScaleNoSelection", "Select a display record to inspect experimental scaling.", "ScaleRequiresExactMatch", "Current scale is readable, but direct changes are disabled until this active display has an Exact association.", "ScaleCurrentUnavailable", "Current per-monitor scale could not be read.", "ScaleNoVerifiedProfile", "Current scale is readable, but no verified compatibility profile authorizes a direct change for this Windows/display mapping. Use Display Settings for now.", "ScaleAvailable", "Experimental scaling is available for this verified profile.",
                "Ready", "Ready. Refresh to scan registered display configurations.", "Recovery", "Recovery and operation history", "RecoveryLocation", "Recovery files are kept here", "OpenRecoveryFolder", "Open recovery folder", "NoOperationYet", "No operation has been recorded in this session.", "RestartRequired", "Restart required", "DisplayRecordsHint", "Rows are registry configurations, not physical-display counts. Exact association is required for direct current-mode changes.",
                "ConnectionActive", "Active", "ConnectionHistorical", "Historical", "ConnectionInactive", "Inactive", "ConnectionConflicted", "Configuration conflict", "ConnectionUnknown", "Unknown", "MatchExact", "Exact", "MatchCandidate", "Candidate", "MatchUnmatched", "Unmatched", "MatchAmbiguous", "Ambiguous", "EvidenceExact", "Unique EDID, monitor identity, and current-mode evidence matched.", "EvidenceTopologyUnique", "Topology and current-mode evidence identify one candidate record, but no stable monitor token proves an Exact association.", "EvidenceCandidate", "Current-mode resolution matches, but the registry key lacks a unique monitor identity token.", "EvidenceUnmatched", "No active Windows display has compatible resolution evidence.", "EvidenceAmbiguous", "Multiple active displays match only resolution evidence.", "EvidenceDuplicate", "Multiple registered configuration roots match the same active Windows display by resolution only.", "WarningCandidate", "Candidate live association only. Current desktop mode, experimental scale, and virtual-capability writes stay disabled until an Exact match is proven.", "WarningUnmatched", "Historical or uncorrelated registry configuration. It remains eligible for virtual-capability planning.", "WarningAmbiguous", "Ambiguous live association. The record is not treated as the current display.", "WarningDuplicate", "Multiple configuration roots point to the same display. Capability changes are disabled until the current registry configuration can be identified.", "WarningNoTarget", "No writable PrimSurfSize target was found.", "ScaleRequiresActiveDisplay", "Select an Active display to change experimental scale.", "ScaleNoCompatibleSettingsTarget", "Current scale is readable, but no unique current-user PerMonitorSettings target could be resolved for this display.", "RestoreScale", "Restore latest scale", "RestoreScaleQuestion", "Restore the latest experimental per-monitor scale journal?", "ConfirmScale", "Apply experimental per-monitor scale {0}%? A .reg backup and a value-level journal will be created first. Sign out or restart Windows after the write.", "ScaleApplied", "Experimental scale written; sign out or restart Windows before expecting it to take effect.", "ScaleRestored", "Experimental scale restored; sign out or restart Windows before expecting it to take effect.", "Unavailable", "Unavailable");
        }

        private static IDictionary<string, string> CreateChinese()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验",
                "Refresh", "刷新", "DisplaySettings", "显示设置", "ExportDiagnostic", "导出诊断包", "DiagnosticExported", "诊断包已导出到：", "DiagnosticExportFailed", "诊断包创建失败。", "ResetDisplayCache", "清理显示缓存（最终修复）", "ResetDisplayCachePrompt", "最终修复方案：程序将先备份，再清理 Windows 的全部显示缓存（GraphicsDrivers\\Configuration、Connectivity 和 ScaleFactors）。Windows 将在立即重启后自动重新生成。请先保存工作。是否继续？", "DisplayCacheResetSuccess", "Windows 显示缓存已清理，备份已保存。系统现在将立即重启。", "DisplayCacheResetFailed", "Windows 显示缓存清理失败，系统不会重启。", "RestartFailed", "无法自动重启 Windows，请手动重启。", "AboutAuthor", "关于作者 CXT（MeowLove）", "Language", "语言",
                "RegisteredConfigurations", "已注册显示配置", "Status", "状态", "Configuration", "配置", "Display", "显示器", "Surface", "表面", "Signal", "信号", "Association", "关联",
                "CapabilityPlan", "虚拟分辨率能力计划", "Magnification", "倍率：", "BuildPlan", "生成计划", "ApplySelected", "应用所选能力", "ApplyAll", "全部应用能力", "RestoreLatest", "恢复最近一次",
                "CurrentState", "当前 Windows 桌面分辨率与缩放", "AvailableModes", "可用桌面模式与驱动模式", "ApplyMode", "测试并应用桌面分辨率", "ExperimentalScale", "实验性每显示器缩放", "ApplyScale", "应用实验性缩放", "ModeNoSelection", "请选择显示配置以检查当前 Windows 分辨率。", "ModeRequiresActive", "直接切换当前分辨率需要选择活动显示配置。", "ModeRequiresConflict", "当前分辨率操作已禁用：多个注册表配置根匹配到同一个 Windows 显示器。", "ModeRequiresExact", "当前分辨率操作已禁用：此显示器关联为“候选”，尚未达到“精确（Exact）”。", "ModeNoLiveDisplay", "该活动显示器没有可用的实时 Windows 显示对象。", "ModeNoModes", "此记录没有可供 Windows 切换的显示模式。", "ModeAvailable", "虚拟桌面模式使用 Windows 显示配置路径；驱动报告模式使用传统驱动路径。", "ModeKindVirtualDesktop", "虚拟桌面", "ModeKindDriver", "驱动报告",
                "ScaleUnavailable", "直接缩放由已验证的兼容配置严格保护。", "CurrentStateGuard", "当前分辨率切换需要 Active + Exact 关联；实验性缩放使用独立的当前用户 PerMonitorSettings 校验。注册表能力修改与当前 Windows 分辨率保持独立。",
                "CapabilityWarning", "虚拟分辨率能力是注册表兼容上限，不等于当前 Windows 分辨率或当前显示缩放。", "KeepMode", "保留此显示模式", "RevertMode", "立即还原",
                "KeepModePrompt", "是否保留新的显示模式？倒计时结束时会自动还原。", "ConfirmCapability", "是否应用已生成的虚拟分辨率能力计划？程序会先创建完整注册表导出和数值级 Journal。新的模式可能需要重启或重新初始化显示驱动后才出现。",
                "ConfirmAll", "是否对全部符合条件的已注册记录应用虚拟分辨率能力计划？一个备份和一个 Journal 将保护整个批次。", "OperationResult", "操作结果", "Error", "错误", "Cancel", "取消", "RestoreQuestion", "是否恢复最近一次虚拟分辨率能力 Journal？", "SecondsRemaining", "剩余 {0} 秒",
                "SelectConfiguration", "选择一个已注册配置以查看其虚拟分辨率能力。", "SelectedSummary", "{0}\n目标节点：{1}\n匹配：{2}；连接状态：{3}\n{4}\n{5}", "NoLiveDisplay", "未选择实时显示器关联。", "CurrentStateSummary", "虚拟分辨率能力：{0}；当前 Windows 分辨率：{1}；当前每显示器缩放：{2}。",
                "NoPlan", "尚未生成计划。", "ScanComplete", "扫描完成：{0} 个配置根节点，{1} 个可写目标节点。", "ScanFailed", "扫描失败：", "PreviewOnly", "仅为预览；未修改任何注册表值。", "PlanBuilt", "分辨率计划已生成。", "PlanUnavailable", "计划不可用：", "PlanValidationFailed", "计划验证失败。",
                "OperationSucceeded", "操作已完成。", "OperationFailed", "操作未完成。", "BackupPathLabel", "注册表备份：", "JournalPathLabel", "Journal：", "RestartRequiredNotice", "需要重启：请先保存工作，然后重启 Windows 或重新初始化显示驱动，新的显示模式才可能出现。",
                "ScaleNoSelection", "请选择显示配置以检查实验性缩放。", "ScaleRequiresExactMatch", "当前缩放已读取，但此活动显示器尚未获得 Exact 关联，因此直接修改被禁用。", "ScaleCurrentUnavailable", "无法读取当前每显示器缩放。", "ScaleNoVerifiedProfile", "当前缩放已读取，但没有已验证的兼容配置允许修改此 Windows/显示器映射。请暂时使用“显示设置”。", "ScaleAvailable", "此已验证配置允许实验性缩放。",
                "Ready", "已就绪。刷新以扫描已注册的显示配置。", "Recovery", "恢复与操作记录", "RecoveryLocation", "恢复文件保存在此处", "OpenRecoveryFolder", "打开恢复目录", "NoOperationYet", "本次会话尚未执行操作。", "RestartRequired", "需要重启", "DisplayRecordsHint", "列表行是注册表显示配置，不等于物理显示器数量；直接修改当前分辨率仍需要 Exact 关联。",
                "ConnectionActive", "活动", "ConnectionHistorical", "历史", "ConnectionInactive", "未活动", "ConnectionConflicted", "配置冲突", "ConnectionUnknown", "未知", "MatchExact", "精确", "MatchCandidate", "候选", "MatchUnmatched", "未关联", "MatchAmbiguous", "存在歧义", "EvidenceExact", "唯一 EDID、显示器身份与当前模式证据均已匹配。", "EvidenceTopologyUnique", "当前活动拓扑与当前模式证据唯一对应一个候选记录，但没有稳定的显示器标记可以证明 Exact 关联。", "EvidenceCandidate", "当前模式分辨率匹配，但注册表键缺少唯一的显示器身份标记。", "EvidenceUnmatched", "没有活动 Windows 显示器具有兼容的分辨率证据。", "EvidenceAmbiguous", "多个活动显示器仅在分辨率证据上匹配。", "EvidenceDuplicate", "多个注册表配置根仅凭分辨率匹配到同一个活动 Windows 显示器。", "WarningCandidate", "仅为候选实时关联。当前桌面分辨率、实验性缩放和虚拟分辨率能力写入均已禁用，直到证明 Exact 关联。", "WarningUnmatched", "历史或未关联的注册表配置，仍可用于生成虚拟分辨率能力计划。", "WarningAmbiguous", "实时关联存在歧义，不会将此记录视为当前显示器。", "WarningDuplicate", "多个配置根指向同一个显示器。未识别出当前生效配置前，已禁用虚拟分辨率能力修改。", "WarningNoTarget", "未找到可写入的 PrimSurfSize 目标。", "ScaleRequiresActiveDisplay", "请选择活动显示器以调整实验性缩放。", "ScaleNoCompatibleSettingsTarget", "当前缩放已读取，但无法为此显示器唯一定位当前用户的 PerMonitorSettings 目标。", "RestoreScale", "恢复最近一次缩放", "RestoreScaleQuestion", "是否恢复最近一次实验性每显示器缩放 Journal？", "ConfirmScale", "是否应用实验性每显示器缩放 {0}%？程序会先创建 .reg 备份和值级 Journal。写入后请注销或重启 Windows。", "ScaleApplied", "实验性缩放已写入；请注销或重启 Windows 后再等待其生效。", "ScaleRestored", "实验性缩放已恢复；请注销或重启 Windows 后再等待其生效。", "Unavailable", "不可用" );
        }

        private static IDictionary<string, string> CreateRussian()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "Более высокое виртуальное разрешение и масштабирование в стиле HiDPI для Windows",
                "Refresh", "Обновить", "DisplaySettings", "Параметры дисплея", "ExportDiagnostic", "Экспорт пакета диагностики", "DiagnosticExported", "Пакет диагностики сохранён в: ", "DiagnosticExportFailed", "Не удалось создать пакет диагностики.", "ResetDisplayCache", "Очистить кэш дисплея (финальное исправление)", "ResetDisplayCachePrompt", "Финальное исправление: WinSuperResolution создаст резервные копии и удалит весь кэш дисплея Windows (GraphicsDrivers\\Configuration, Connectivity и ScaleFactors). Windows восстановит его после немедленной перезагрузки. Сохраните работу. Продолжить?", "DisplayCacheResetSuccess", "Кэш дисплея Windows очищен, резервные копии сохранены. Windows будет немедленно перезапущена.", "DisplayCacheResetFailed", "Не удалось очистить кэш дисплея Windows. Перезагрузка не выполнялась.", "RestartFailed", "Не удалось автоматически перезапустить Windows. Перезапустите Windows вручную.", "AboutAuthor", "Об авторе CXT (MeowLove)", "Language", "Язык",
                "RegisteredConfigurations", "Зарегистрированные конфигурации дисплея", "Status", "Статус", "Configuration", "Конфигурация", "Display", "Дисплей", "Surface", "Поверхность", "Signal", "Сигнал", "Association", "Связь",
                "CapabilityPlan", "План виртуального разрешения", "Magnification", "Масштаб:", "BuildPlan", "Создать план", "ApplySelected", "Применить выбранную возможность", "ApplyAll", "Применить ко всем", "RestoreLatest", "Восстановить последнее",
                "CurrentState", "Текущий режим рабочего стола Windows и масштаб", "AvailableModes", "Доступные режимы рабочего стола и драйвера", "ApplyMode", "Проверить и применить режим рабочего стола", "ExperimentalScale", "Экспериментальный масштаб для монитора", "ApplyScale", "Применить экспериментальный масштаб", "ModeNoSelection", "Выберите конфигурацию дисплея для проверки текущих режимов Windows.", "ModeRequiresActive", "Для изменения текущего режима требуется активная конфигурация дисплея.", "ModeRequiresConflict", "Изменение текущего режима отключено: несколько конфигураций реестра соответствуют одному дисплею Windows.", "ModeRequiresExact", "Изменение текущего режима отключено: связь дисплея Candidate, а не Exact.", "ModeNoLiveDisplay", "Для активного дисплея нет доступного объекта Windows в реальном времени.", "ModeNoModes", "Для этой записи нет режимов Windows, доступных для переключения.", "ModeAvailable", "Виртуальные режимы рабочего стола используют путь конфигурации дисплея Windows; режимы драйвера используют традиционный путь драйвера.", "ModeKindVirtualDesktop", "Виртуальный рабочий стол", "ModeKindDriver", "Режим драйвера",
                "ScaleUnavailable", "Прямое управление масштабом защищено проверенным профилем совместимости.", "CurrentStateGuard", "Для смены текущего режима требуется связь Active + Exact; экспериментальный масштаб использует отдельную проверку PerMonitorSettings текущего пользователя. Возможность реестра не равна текущему режиму Windows.",
                "CapabilityWarning", "Возможность виртуального разрешения — это предел совместимости реестра, а не текущее разрешение Windows или масштаб дисплея.", "KeepMode", "Сохранить этот режим", "RevertMode", "Вернуть сейчас",
                "KeepModePrompt", "Сохранить новый режим дисплея? По окончании отсчёта он будет восстановлен автоматически.", "ConfirmCapability", "Применить подготовленный план возможности виртуального разрешения? Сначала будут созданы полный экспорт реестра и журнал значений. Новые режимы могут появиться после перезапуска Windows или драйвера дисплея.",
                "ConfirmAll", "Применить план возможности виртуального разрешения ко всем подходящим зарегистрированным записям? Одна резервная копия и один журнал защитят весь пакет.", "OperationResult", "Результат операции", "Error", "Ошибка", "Cancel", "Отмена", "RestoreQuestion", "Восстановить последний журнал возможности виртуального разрешения?", "SecondsRemaining", "Осталось секунд: {0}",
                "SelectConfiguration", "Выберите зарегистрированную конфигурацию, чтобы просмотреть её возможность виртуального разрешения.", "SelectedSummary", "{0}\nЦели: {1}\nСвязь: {2}; подключение: {3}\n{4}\n{5}", "NoLiveDisplay", "Текущий дисплей не выбран.", "CurrentStateSummary", "Возможность виртуального разрешения: {0}; текущий режим Windows: {1}; текущий масштаб монитора: {2}.",
                "NoPlan", "План не создан.", "ScanComplete", "Сканирование завершено: корней конфигурации: {0}; доступных для записи целей: {1}.", "ScanFailed", "Ошибка сканирования: ", "PreviewOnly", "Только предварительный просмотр; значения реестра не изменены.", "PlanBuilt", "План разрешения создан.", "PlanUnavailable", "План недоступен: ", "PlanValidationFailed", "Ошибка проверки плана.",
                "OperationSucceeded", "Операция завершена.", "OperationFailed", "Операция не завершена.", "BackupPathLabel", "Резервная копия реестра: ", "JournalPathLabel", "Журнал: ", "RestartRequiredNotice", "Требуется перезапуск: сохраните работу, затем перезапустите Windows или драйвер дисплея, прежде чем ожидать новые режимы.",
                "ScaleNoSelection", "Выберите конфигурацию дисплея для проверки экспериментального масштаба.", "ScaleRequiresExactMatch", "Текущий масштаб прочитан, но прямое изменение отключено, пока активный дисплей не имеет связи Exact.", "ScaleCurrentUnavailable", "Не удалось прочитать текущий масштаб для монитора.", "ScaleNoVerifiedProfile", "Текущий масштаб прочитан, но нет проверенного профиля совместимости для изменения этой связки Windows и дисплея. Пока используйте параметры дисплея.", "ScaleAvailable", "Для этого проверенного профиля доступен экспериментальный масштаб.",
                "Ready", "Готово. Обновите, чтобы просканировать зарегистрированные конфигурации дисплея.", "Recovery", "Восстановление и история операций", "RecoveryLocation", "Файлы восстановления хранятся здесь", "OpenRecoveryFolder", "Открыть папку восстановления", "NoOperationYet", "В этом сеансе операций пока не было.", "RestartRequired", "Требуется перезапуск", "DisplayRecordsHint", "Строки представляют конфигурации реестра, а не число физических дисплеев. Для прямой смены текущего режима требуется связь Exact.",
                "ConnectionActive", "Активный", "ConnectionHistorical", "Исторический", "ConnectionInactive", "Неактивный", "ConnectionConflicted", "Конфликт конфигураций", "ConnectionUnknown", "Неизвестный", "MatchExact", "Точное", "MatchCandidate", "Кандидат", "MatchUnmatched", "Нет связи", "MatchAmbiguous", "Неоднозначно", "EvidenceExact", "Совпали уникальные EDID, идентификатор монитора и текущий режим.", "EvidenceTopologyUnique", "Активная топология и текущий режим указывают на одну запись-кандидат, но без стабильного идентификатора монитора связь Exact не подтверждена.", "EvidenceCandidate", "Разрешение текущего режима совпадает, но в ключе реестра нет уникального идентификатора монитора.", "EvidenceUnmatched", "Нет активного дисплея Windows с совместимым доказательством разрешения.", "EvidenceAmbiguous", "Несколько активных дисплеев совпадают только по разрешению.", "EvidenceDuplicate", "Несколько корней конфигурации реестра совпадают с одним активным дисплеем Windows только по разрешению.", "WarningCandidate", "Только кандидат на связь с текущим дисплеем. Текущий режим, экспериментальный масштаб и запись виртуальных возможностей отключены до подтверждения Exact.", "WarningUnmatched", "Историческая или несвязанная конфигурация реестра; для неё всё ещё можно создать план виртуального разрешения.", "WarningAmbiguous", "Связь с текущим дисплеем неоднозначна; запись не считается текущим дисплеем.", "WarningDuplicate", "Несколько корней конфигурации указывают на один дисплей. Изменение возможностей отключено, пока не будет определена текущая конфигурация реестра.", "WarningNoTarget", "Не найден доступный для записи целевой PrimSurfSize.", "ScaleRequiresActiveDisplay", "Выберите активный дисплей для изменения экспериментального масштаба.", "ScaleNoCompatibleSettingsTarget", "Текущий масштаб прочитан, но для этого дисплея нельзя однозначно определить текущую цель пользователя PerMonitorSettings.", "RestoreScale", "Восстановить последний масштаб", "RestoreScaleQuestion", "Восстановить последний журнал экспериментального масштаба для монитора?", "ConfirmScale", "Применить экспериментальный масштаб монитора {0}%? Сначала будут созданы резервная копия .reg и журнал значений. После записи выйдите из системы или перезапустите Windows.", "ScaleApplied", "Экспериментальный масштаб записан; выйдите из системы или перезапустите Windows, прежде чем ожидать его применения.", "ScaleRestored", "Экспериментальный масштаб восстановлен; выйдите из системы или перезапустите Windows, прежде чем ожидать его применения.", "Unavailable", "Недоступно");
        }

        private static IDictionary<string, string> Create(params string[] values)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int index = 0; index < values.Length; index += 2)
                dictionary.Add(values[index], values[index + 1]);
            return dictionary;
        }
    }
}
