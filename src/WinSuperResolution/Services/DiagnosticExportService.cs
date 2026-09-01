using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Win32;

namespace WinSuperResolution.Services
{
    internal sealed class DiagnosticExportService
    {
        private const string ConfigurationRegistryPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";

        internal DiagnosticExportResult Export(string summary)
        {
            List<string> failures = new List<string>();
            string diagnosticsDirectory = Path.Combine(AppPaths.ExecutableDirectory, "diagnostics");
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
            string stagingDirectory = Path.Combine(diagnosticsDirectory, "staging-" + stamp + "-" + Guid.NewGuid().ToString("N"));
            string archivePath = Path.Combine(diagnosticsDirectory, "WinSuperResolution-diagnostic-" + stamp + ".zip");

            try
            {
                Directory.CreateDirectory(diagnosticsDirectory);
                Directory.CreateDirectory(stagingDirectory);
                WriteText(stagingDirectory, "summary.txt", BuildSummary(summary, failures));
                WriteText(stagingDirectory, "manifest.txt", BuildManifest());
                CopyOptionalFile(Path.Combine(AppPaths.LogsDirectory, "WinSuperResolution.log"), stagingDirectory, "logs/WinSuperResolution.log", failures);
                CopyOptionalDirectory(AppPaths.JournalsDirectory, stagingDirectory, "journals", failures);
                CopyOptionalDirectory(AppPaths.BackupsDirectory, stagingDirectory, "registry-backups", failures);
                CopyOptionalDirectory(AppPaths.DisplayStateDirectory, stagingDirectory, "display-state", failures);
                ExportConfigurationRegistry(Path.Combine(stagingDirectory, "registry", "GraphicsDrivers-Configuration.reg"), failures);
                WriteText(stagingDirectory, "summary.txt", BuildSummary(summary, failures));
                ZipFile.CreateFromDirectory(stagingDirectory, archivePath, CompressionLevel.Optimal, false);
                return new DiagnosticExportResult(true, archivePath, failures);
            }
            catch (Exception exception)
            {
                failures.Add("Archive creation failed: " + exception.Message);
                return new DiagnosticExportResult(false, archivePath, failures);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(stagingDirectory))
                        Directory.Delete(stagingDirectory, true);
                }
                catch
                {
                    // The archive is already usable; retain no staging cleanup error in the UI.
                }
            }
        }

        private static string BuildManifest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            return "Created: " + DateTimeOffset.Now.ToString("o") + Environment.NewLine
                + "Application: WinSuperResolution" + Environment.NewLine
                + "AssemblyVersion: " + (version == null ? "unknown" : version.ToString()) + Environment.NewLine
                + "OSVersion: " + Environment.OSVersion.Version + Environment.NewLine
                + "64BitOS: " + Environment.Is64BitOperatingSystem + Environment.NewLine
                + "64BitProcess: " + Environment.Is64BitProcess + Environment.NewLine;
        }

        private static string BuildSummary(string summary, IList<string> failures)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("WinSuperResolution diagnostic package");
            writer.WriteLine("Generated: " + DateTimeOffset.Now.ToString("o"));
            writer.WriteLine();
            writer.WriteLine(summary ?? string.Empty);
            writer.WriteLine();
            writer.WriteLine("Included artifacts:");
            writer.WriteLine("- manifest.txt");
            writer.WriteLine("- logs/WinSuperResolution.log (when available)");
            writer.WriteLine("- journals/, registry-backups/, display-state/ (when available)");
            writer.WriteLine("- registry/GraphicsDrivers-Configuration.reg");
            if (failures.Count > 0)
            {
                writer.WriteLine();
                writer.WriteLine("Optional collection failures:");
                foreach (string failure in failures)
                    writer.WriteLine("- " + failure);
            }
            return writer.ToString();
        }

        private static void CopyOptionalDirectory(string sourceDirectory, string stagingDirectory, string packageDirectory, IList<string> failures)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                failures.Add(packageDirectory + ": source directory not found (" + sourceDirectory + ")");
                return;
            }
            try
            {
                foreach (string sourcePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    string relative = sourcePath.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    CopyOptionalFile(sourcePath, stagingDirectory, packageDirectory + "/" + relative.Replace('\\', '/'), failures);
                }
            }
            catch (Exception exception)
            {
                failures.Add(packageDirectory + ": " + exception.Message);
            }
        }

        private static void CopyOptionalFile(string sourcePath, string stagingDirectory, string packagePath, IList<string> failures)
        {
            if (!File.Exists(sourcePath))
            {
                failures.Add(packagePath + ": source file not found (" + sourcePath + ")");
                return;
            }
            try
            {
                string destinationPath = Path.Combine(stagingDirectory, packagePath.Replace('/', Path.DirectorySeparatorChar));
                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);
                File.Copy(sourcePath, destinationPath, true);
            }
            catch (Exception exception)
            {
                failures.Add(packagePath + ": " + exception.Message);
            }
        }

        private static void WriteText(string stagingDirectory, string packagePath, string content)
        {
            string destinationPath = Path.Combine(stagingDirectory, packagePath.Replace('/', Path.DirectorySeparatorChar));
            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);
            File.WriteAllText(destinationPath, content ?? string.Empty);
        }

        private static void ExportConfigurationRegistry(string outputPath, IList<string> failures)
        {
            try
            {
                string directory = Path.GetDirectoryName(outputPath);
                Directory.CreateDirectory(directory);
                using (StreamWriter writer = new StreamWriter(outputPath, false, System.Text.Encoding.Unicode))
                using (RegistryKey root = Registry.LocalMachine.OpenSubKey(ConfigurationRegistryPath, false))
                {
                    writer.WriteLine("Windows Registry Editor Version 5.00");
                    writer.WriteLine();
                    if (root == null)
                    {
                        writer.WriteLine("; Registry key unavailable: HKLM\\" + ConfigurationRegistryPath);
                        failures.Add("registry/GraphicsDrivers-Configuration.reg: registry key unavailable");
                        return;
                    }
                    WriteRegistryKey(writer, root, @"HKEY_LOCAL_MACHINE\" + ConfigurationRegistryPath);
                }
            }
            catch (Exception exception)
            {
                failures.Add("registry/GraphicsDrivers-Configuration.reg: " + exception.Message);
                try
                {
                    File.WriteAllText(outputPath, "Windows Registry Editor Version 5.00" + Environment.NewLine + "; Export failed: " + exception.Message, System.Text.Encoding.Unicode);
                }
                catch
                {
                    // The failure is already reported in summary.txt.
                }
            }
        }

        private static void WriteRegistryKey(StreamWriter writer, RegistryKey key, string fullPath)
        {
            writer.WriteLine("[" + fullPath + "]");
            foreach (string valueName in key.GetValueNames())
            {
                object value = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                writer.WriteLine("\"" + Escape(valueName) + "\"=" + FormatRegistryValue(key.GetValueKind(valueName), value));
            }
            writer.WriteLine();
            foreach (string childName in key.GetSubKeyNames())
            {
                using (RegistryKey child = key.OpenSubKey(childName, false))
                {
                    if (child != null)
                        WriteRegistryKey(writer, child, fullPath + "\\" + childName);
                }
            }
        }

        private static string FormatRegistryValue(RegistryValueKind kind, object value)
        {
            if (kind == RegistryValueKind.DWord)
                return "dword:" + Convert.ToUInt32(value).ToString("x8");
            if (kind == RegistryValueKind.QWord)
                return "hex(b):" + BitConverter.ToString(BitConverter.GetBytes(Convert.ToUInt64(value))).Replace("-", ",").ToLowerInvariant();
            if (kind == RegistryValueKind.Binary)
                return "hex:" + BitConverter.ToString((byte[])value).Replace("-", ",").ToLowerInvariant();
            if (kind == RegistryValueKind.MultiString)
                return "hex(7):" + BitConverter.ToString(System.Text.Encoding.Unicode.GetBytes(string.Join("\0", (string[])value) + "\0\0")).Replace("-", ",").ToLowerInvariant();
            return "\"" + Escape(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)) + "\"";
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }

    internal sealed class DiagnosticExportResult
    {
        internal DiagnosticExportResult(bool succeeded, string archivePath, IList<string> failures)
        {
            Succeeded = succeeded;
            ArchivePath = archivePath;
            Failures = failures;
        }

        internal bool Succeeded { get; private set; }
        internal string ArchivePath { get; private set; }
        internal IList<string> Failures { get; private set; }
    }
}
