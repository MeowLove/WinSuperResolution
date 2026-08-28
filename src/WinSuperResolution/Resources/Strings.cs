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
                "Refresh", "Refresh", "DisplaySettings", "Display Settings", "CopyDiagnostic", "Copy Diagnostic", "Language", "Language",
                "RegisteredConfigurations", "Registered display configurations", "Status", "Status", "Configuration", "Configuration", "Display", "Display", "Surface", "Surface", "Signal", "Signal",
                "CapabilityPlan", "Virtual-resolution capability plan", "Magnification", "Magnification:", "BuildPlan", "Build Plan", "ApplySelected", "Apply selected capability", "ApplyAll", "Apply all capabilities", "RestoreLatest", "Restore latest",
                "CurrentState", "Current Windows mode and scale", "AvailableModes", "Windows-supported modes", "ApplyMode", "Test & apply current mode", "ExperimentalScale", "Experimental per-monitor scale", "ApplyScale", "Apply experimental scale",
                "ScaleUnavailable", "Direct scale control is guarded by a verified compatibility profile.", "CurrentStateGuard", "Current mode and scale are available only for an Active + Exact association. Registry capability edits remain separate from the current Windows mode.",
                "CapabilityWarning", "Virtual-resolution capability is a registry compatibility ceiling, not the current Windows resolution or current display scale.", "KeepMode", "Keep this display mode", "RevertMode", "Revert now",
                "KeepModePrompt", "Keep the new display mode? It will be restored automatically when the countdown ends.", "ConfirmCapability", "Apply the prepared virtual-resolution capability plan? A full registry export and a value-level journal will be created first. Windows may need a restart or display-driver reinitialization before new modes appear.",
                "ConfirmAll", "Apply the prepared virtual-resolution capability plan to all eligible registered records? One backup and one journal will protect the batch.", "OperationResult", "Operation result", "Error", "Error", "Cancel", "Cancel", "RestoreQuestion", "Restore the latest virtual-resolution capability journal?", "DiagnosticCopied", "Diagnostic summary copied to the clipboard.", "SecondsRemaining", "{0} seconds remaining");
        }

        private static IDictionary<string, string> CreateChinese()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验",
                "Refresh", "刷新", "DisplaySettings", "显示设置", "CopyDiagnostic", "复制诊断摘要", "Language", "语言",
                "RegisteredConfigurations", "已注册显示配置", "Status", "状态", "Configuration", "配置", "Display", "显示器", "Surface", "表面", "Signal", "信号",
                "CapabilityPlan", "虚拟分辨率能力计划", "Magnification", "倍率：", "BuildPlan", "生成计划", "ApplySelected", "应用所选能力", "ApplyAll", "全部应用能力", "RestoreLatest", "恢复最近一次",
                "CurrentState", "当前 Windows 分辨率与缩放", "AvailableModes", "Windows 当前支持的模式", "ApplyMode", "测试并应用当前分辨率", "ExperimentalScale", "实验性每显示器缩放", "ApplyScale", "应用实验性缩放",
                "ScaleUnavailable", "直接缩放由已验证的兼容配置严格保护。", "CurrentStateGuard", "当前分辨率和缩放仅对 Active + Exact 关联开放。注册表能力修改与当前 Windows 分辨率保持独立。",
                "CapabilityWarning", "虚拟分辨率能力是注册表兼容上限，不等于当前 Windows 分辨率或当前显示缩放。", "KeepMode", "保留此显示模式", "RevertMode", "立即还原",
                "KeepModePrompt", "是否保留新的显示模式？倒计时结束时会自动还原。", "ConfirmCapability", "是否应用已生成的虚拟分辨率能力计划？程序会先创建完整注册表导出和数值级 Journal。新的模式可能需要重启或重新初始化显示驱动后才出现。",
                "ConfirmAll", "是否对全部符合条件的已注册记录应用虚拟分辨率能力计划？一个备份和一个 Journal 将保护整个批次。", "OperationResult", "操作结果", "Error", "错误", "Cancel", "取消", "RestoreQuestion", "是否恢复最近一次虚拟分辨率能力 Journal？", "DiagnosticCopied", "诊断摘要已复制到剪贴板。", "SecondsRemaining", "剩余 {0} 秒");
        }

        private static IDictionary<string, string> CreateRussian()
        {
            return Create(
                "ProductName", "WinSuperResolution", "Subtitle", "Более высокое виртуальное разрешение и масштабирование в стиле HiDPI для Windows",
                "Refresh", "Обновить", "DisplaySettings", "Параметры дисплея", "CopyDiagnostic", "Копировать диагностику", "Language", "Язык",
                "RegisteredConfigurations", "Зарегистрированные конфигурации дисплея", "Status", "Статус", "Configuration", "Конфигурация", "Display", "Дисплей", "Surface", "Поверхность", "Signal", "Сигнал",
                "CapabilityPlan", "План виртуального разрешения", "Magnification", "Масштаб:", "BuildPlan", "Создать план", "ApplySelected", "Применить выбранную возможность", "ApplyAll", "Применить ко всем", "RestoreLatest", "Восстановить последнее",
                "CurrentState", "Текущий режим Windows и масштаб", "AvailableModes", "Режимы, доступные в Windows", "ApplyMode", "Проверить и применить режим", "ExperimentalScale", "Экспериментальный масштаб для монитора", "ApplyScale", "Применить экспериментальный масштаб",
                "ScaleUnavailable", "Прямое управление масштабом защищено проверенным профилем совместимости.", "CurrentStateGuard", "Текущий режим и масштаб доступны только при связи Active + Exact. Изменение возможности реестра не равно текущему режиму Windows.",
                "CapabilityWarning", "Возможность виртуального разрешения — это предел совместимости реестра, а не текущее разрешение Windows или масштаб дисплея.", "KeepMode", "Сохранить этот режим", "RevertMode", "Вернуть сейчас",
                "KeepModePrompt", "Сохранить новый режим дисплея? По окончании отсчёта он будет восстановлен автоматически.", "ConfirmCapability", "Применить подготовленный план возможности виртуального разрешения? Сначала будут созданы полный экспорт реестра и журнал значений. Новые режимы могут появиться после перезапуска Windows или драйвера дисплея.",
                "ConfirmAll", "Применить план возможности виртуального разрешения ко всем подходящим зарегистрированным записям? Одна резервная копия и один журнал защитят весь пакет.", "OperationResult", "Результат операции", "Error", "Ошибка", "Cancel", "Отмена", "RestoreQuestion", "Восстановить последний журнал возможности виртуального разрешения?", "DiagnosticCopied", "Сводка диагностики скопирована в буфер обмена.", "SecondsRemaining", "Осталось секунд: {0}");
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
