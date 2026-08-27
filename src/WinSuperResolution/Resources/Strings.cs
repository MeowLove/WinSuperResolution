using System.Collections.Generic;

namespace WinSuperResolution.Resources
{
    internal static class Strings
    {
        internal const string DefaultCulture = "en-US";

        internal static readonly IDictionary<string, IDictionary<string, string>> Values =
            new Dictionary<string, IDictionary<string, string>>
            {
                { "en-US", new Dictionary<string, string> { { "ProductName", "WinSuperResolution" }, { "Refresh", "Refresh" } } },
                { "zh-CN", new Dictionary<string, string> { { "ProductName", "WinSuperResolution" }, { "Refresh", "刷新" } } },
                { "ru-RU", new Dictionary<string, string> { { "ProductName", "WinSuperResolution" }, { "Refresh", "Обновить" } } }
            };
    }
}
