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

        private static readonly IDictionary<string, IDictionary<string, string>> Values =
            new Dictionary<string, IDictionary<string, string>>
            {
                { "en-US", CreateEnglish() },
                { "zh-CN", CreateChinese() },
                { "ru-RU", CreateRussian() }
            };

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
                "Refresh", "Refresh", "DisplaySettings", "Display Settings", "ExportDiagnostic", "Export Diagnostic Package", "DiagnosticExported", "Diagnostic package exported to: ", "DiagnosticExportFailed", "Diagnostic package could not be created.", "ResetDisplayCache", "Reset display cache (final repair)", "ResetDisplayCachePrompt", "Final repair: WinSuperResolution will back up and delete all Windows display caches (GraphicsDrivers\\Configuration, Connectivity, and ScaleFactors). Windows will rebuild them after an immediate restart. Save your work. Continue?", "DisplayCacheResetSuccess", "Windows display caches were cleared and backups were saved. Windows will restart now.", "DisplayCacheResetFailed", "The Windows display caches could not be cleared. No restart was requested.", "RestartFailed", "Windows could not be restarted automatically. Restart Windows manually.", "Language", "Language",
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
                "ConnectionActive", "Active", "ConnectionHistorical", "Historical", "ConnectionInactive", "Inactive", "ConnectionConflicted", "Configuration conflict", "ConnectionUnknown", "Unknown", "MatchExact", "Exact", "MatchCandidate", "Candidate", "MatchUnmatched", "Unmatched", "MatchAmbiguous", "Ambiguous", "EvidenceExact", "Unique EDID, monitor identity, and current-mode evidence matched.", "EvidenceTopologyUnique", "The active topology and current-mode evidence identify one registered record; the registry key has no stable monitor token.", "EvidenceCandidate", "Current-mode resolution matches, but the registry key lacks a unique monitor identity token.", "EvidenceUnmatched", "No active Windows display has compatible resolution evidence.", "EvidenceAmbiguous", "Multiple active displays match only resolution evidence.", "EvidenceDuplicate", "Multiple registered configuration roots match the same active Windows display by resolution only.", "WarningCandidate", "Candidate live association only. Direct current-mode control stays disabled until an Exact match is proven; experimental scale follows its own current-user target check.", "WarningUnmatched", "Historical or uncorrelated registry configuration. It remains eligible for virtual-capability planning.", "WarningAmbiguous", "Ambiguous live association. The record is not treated as the current display.", "WarningDuplicate", "Multiple configuration roots point to the same display. Capability changes are disabled until the current registry configuration can be identified.", "WarningNoTarget", "No writable PrimSurfSize target was found.", "ScaleRequiresActiveDisplay", "Select an Active display to change experimental scale.", "ScaleNoCompatibleSettingsTarget", "Current scale is readable, but no unique current-user PerMonitorSettings target could be resolved for this display.", "RestoreScale", "Restore latest scale", "RestoreScaleQuestion", "Restore the latest experimental per-monitor scale journal?", "ConfirmScale", "Apply experimental per-monitor scale {0}%? A .reg backup and a value-level journal will be created first. Sign out or restart Windows after the write.", "ScaleApplied", "Experimental scale written; sign out or restart Windows before expecting it to take effect.", "ScaleRestored", "Experimental scale restored; sign out or restart Windows before expecting it to take effect.", "Unavailable", "Unavailable");
        }

        private static IDictionary<string, string> CreateChinese()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验",
                "Refresh", "刷新", "DisplaySettings", "显示设置", "ExportDiagnostic", "导出诊断包", "DiagnosticExported", "诊断包已导出到：", "DiagnosticExportFailed", "诊断包创建失败。", "ResetDisplayCache", "清理显示缓存（最终修复）", "ResetDisplayCachePrompt", "最终修复方案：程序将先备份，再清理 Windows 的全部显示缓存（GraphicsDrivers\\Configuration、Connectivity 和 ScaleFactors）。Windows 将在立即重启后自动重新生成。请先保存工作。是否继续？", "DisplayCacheResetSuccess", "Windows 显示缓存已清理，备份已保存。系统现在将立即重启。", "DisplayCacheResetFailed", "Windows 显示缓存清理失败，系统不会重启。", "RestartFailed", "无法自动重启 Windows，请手动重启。", "Language", "语言",
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
                "ConnectionActive", "活动", "ConnectionHistorical", "历史", "ConnectionInactive", "未活动", "ConnectionConflicted", "配置冲突", "ConnectionUnknown", "未知", "MatchExact", "精确", "MatchCandidate", "候选", "MatchUnmatched", "未关联", "MatchAmbiguous", "存在歧义", "EvidenceExact", "唯一 EDID、显示器身份与当前模式证据均已匹配。", "EvidenceTopologyUnique", "当前活动拓扑与当前模式证据唯一对应一个已注册记录，但注册表键没有稳定的显示器标记。", "EvidenceCandidate", "当前模式分辨率匹配，但注册表键缺少唯一的显示器身份标记。", "EvidenceUnmatched", "没有活动 Windows 显示器具有兼容的分辨率证据。", "EvidenceAmbiguous", "多个活动显示器仅在分辨率证据上匹配。", "EvidenceDuplicate", "多个注册表配置根仅凭分辨率匹配到同一个活动 Windows 显示器。", "WarningCandidate", "仅为候选实时关联。直接修改当前分辨率仍需证明 Exact 关联；实验性缩放使用独立的当前用户目标校验。", "WarningUnmatched", "历史或未关联的注册表配置，仍可用于生成虚拟分辨率能力计划。", "WarningAmbiguous", "实时关联存在歧义，不会将此记录视为当前显示器。", "WarningDuplicate", "多个配置根指向同一个显示器。未识别出当前生效配置前，已禁用虚拟分辨率能力修改。", "WarningNoTarget", "未找到可写入的 PrimSurfSize 目标。", "ScaleRequiresActiveDisplay", "请选择活动显示器以调整实验性缩放。", "ScaleNoCompatibleSettingsTarget", "当前缩放已读取，但无法为此显示器唯一定位当前用户的 PerMonitorSettings 目标。", "RestoreScale", "恢复最近一次缩放", "RestoreScaleQuestion", "是否恢复最近一次实验性每显示器缩放 Journal？", "ConfirmScale", "是否应用实验性每显示器缩放 {0}%？程序会先创建 .reg 备份和值级 Journal。写入后请注销或重启 Windows。", "ScaleApplied", "实验性缩放已写入；请注销或重启 Windows 后再等待其生效。", "ScaleRestored", "实验性缩放已恢复；请注销或重启 Windows 后再等待其生效。", "Unavailable", "不可用" );
        }

        private static IDictionary<string, string> CreateRussian()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "Более высокое виртуальное разрешение и масштабирование в стиле HiDPI для Windows",
                "Refresh", "Обновить", "DisplaySettings", "Параметры дисплея", "ExportDiagnostic", "Экспорт пакета диагностики", "DiagnosticExported", "Пакет диагностики сохранён в: ", "DiagnosticExportFailed", "Не удалось создать пакет диагностики.", "ResetDisplayCache", "Очистить кэш дисплея (финальное исправление)", "ResetDisplayCachePrompt", "Финальное исправление: WinSuperResolution создаст резервные копии и удалит весь кэш дисплея Windows (GraphicsDrivers\\Configuration, Connectivity и ScaleFactors). Windows восстановит его после немедленной перезагрузки. Сохраните работу. Продолжить?", "DisplayCacheResetSuccess", "Кэш дисплея Windows очищен, резервные копии сохранены. Windows будет немедленно перезапущена.", "DisplayCacheResetFailed", "Не удалось очистить кэш дисплея Windows. Перезагрузка не выполнялась.", "RestartFailed", "Не удалось автоматически перезапустить Windows. Перезапустите Windows вручную.", "Language", "Язык",
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
                "ConnectionActive", "Активный", "ConnectionHistorical", "Исторический", "ConnectionInactive", "Неактивный", "ConnectionConflicted", "Конфликт конфигураций", "ConnectionUnknown", "Неизвестный", "MatchExact", "Точное", "MatchCandidate", "Кандидат", "MatchUnmatched", "Нет связи", "MatchAmbiguous", "Неоднозначно", "EvidenceExact", "Совпали уникальные EDID, идентификатор монитора и текущий режим.", "EvidenceTopologyUnique", "Активная топология и текущий режим однозначно указывают на одну зарегистрированную запись; в ключе реестра нет стабильного идентификатора монитора.", "EvidenceCandidate", "Разрешение текущего режима совпадает, но в ключе реестра нет уникального идентификатора монитора.", "EvidenceUnmatched", "Нет активного дисплея Windows с совместимым доказательством разрешения.", "EvidenceAmbiguous", "Несколько активных дисплеев совпадают только по разрешению.", "EvidenceDuplicate", "Несколько корней конфигурации реестра совпадают с одним активным дисплеем Windows только по разрешению.", "WarningCandidate", "Только кандидат на связь с текущим дисплеем. Прямой текущий режим остаётся отключённым до Exact; экспериментальный масштаб использует отдельную проверку текущего пользователя.", "WarningUnmatched", "Историческая или несвязанная конфигурация реестра; для неё всё ещё можно создать план виртуального разрешения.", "WarningAmbiguous", "Связь с текущим дисплеем неоднозначна; запись не считается текущим дисплеем.", "WarningDuplicate", "Несколько корней конфигурации указывают на один дисплей. Изменение возможностей отключено, пока не будет определена текущая конфигурация реестра.", "WarningNoTarget", "Не найден доступный для записи целевой PrimSurfSize.", "ScaleRequiresActiveDisplay", "Выберите активный дисплей для изменения экспериментального масштаба.", "ScaleNoCompatibleSettingsTarget", "Текущий масштаб прочитан, но для этого дисплея нельзя однозначно определить текущую цель пользователя PerMonitorSettings.", "RestoreScale", "Восстановить последний масштаб", "RestoreScaleQuestion", "Восстановить последний журнал экспериментального масштаба для монитора?", "ConfirmScale", "Применить экспериментальный масштаб монитора {0}%? Сначала будут созданы резервная копия .reg и журнал значений. После записи выйдите из системы или перезапустите Windows.", "ScaleApplied", "Экспериментальный масштаб записан; выйдите из системы или перезапустите Windows, прежде чем ожидать его применения.", "ScaleRestored", "Экспериментальный масштаб восстановлен; выйдите из системы или перезапустите Windows, прежде чем ожидать его применения.", "Unavailable", "Недоступно");
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
