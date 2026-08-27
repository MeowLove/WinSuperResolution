using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace WinSuperResolution.Services
{
    internal sealed class DiagnosticsService
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern void OutputDebugString(string message);

        internal void Write(string message)
        {
            string line = DateTimeOffset.Now.ToString("o") + " " + message;
            OutputDebugString(line + Environment.NewLine);

            try
            {
                AppPaths.EnsureWritableDataDirectories();
                File.AppendAllText(Path.Combine(AppPaths.LogsDirectory, "WinSuperResolution.log"), line + Environment.NewLine);
            }
            catch
            {
                // Diagnostic logging must never prevent read-only scanning.
            }
        }
    }
}
