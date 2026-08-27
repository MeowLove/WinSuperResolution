using System;
using System.IO;
using System.Text.RegularExpressions;
using WinSuperResolution.Resources;

namespace WinSuperResolution.Services
{
    internal sealed class PortableSettingsService
    {
        internal string LoadLanguage()
        {
            try
            {
                if (!File.Exists(AppPaths.SettingsPath))
                {
                    return Strings.DefaultCulture;
                }

                string content = File.ReadAllText(AppPaths.SettingsPath);
                Match match = Regex.Match(content, "\\\"language\\\"\\s*:\\s*\\\"(?<value>[^\\\"]+)\\\"");
                return match.Success ? match.Groups["value"].Value : Strings.DefaultCulture;
            }
            catch
            {
                return Strings.DefaultCulture;
            }
        }

        internal bool SaveLanguage(string language)
        {
            try
            {
                File.WriteAllText(AppPaths.SettingsPath, "{\"language\":\"" + language + "\"}");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
