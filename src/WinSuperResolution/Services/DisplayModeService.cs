using System;
using System.Collections.Generic;
using System.IO;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class DisplayModeService
    {
        private const int EnumCurrentSettings = -1;
        private const int CdsTest = 0x00000002;
        private const int CdsUpdateRegistry = 0x00000001;
        private const int DispChangeSuccessful = 0;
        private readonly JournalService _journals;

        internal DisplayModeService(JournalService journals)
        {
            _journals = journals;
        }

        internal IList<DisplayMode> EnumerateModes(string deviceName)
        {
            List<DisplayMode> modes = new List<DisplayMode>();
            if (string.IsNullOrEmpty(deviceName))
                return modes;

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; ; index++)
            {
                LiveDisplayService.DevMode value = LiveDisplayService.CreateDevMode();
                if (!LiveDisplayService.EnumDisplaySettingsEx(deviceName, index, ref value, 0))
                    break;
                if (value.PelsWidth <= 0 || value.PelsHeight <= 0)
                    continue;
                DisplayMode mode = new DisplayMode
                {
                    DeviceName = deviceName,
                    Width = value.PelsWidth,
                    Height = value.PelsHeight,
                    Frequency = value.DisplayFrequency,
                    BitsPerPixel = value.BitsPerPixel
                };
                string key = mode.Width + "x" + mode.Height + "@" + mode.Frequency + ":" + mode.BitsPerPixel;
                if (seen.Add(key))
                    modes.Add(mode);
            }
            return modes;
        }

        internal OperationResult ApplyWithSnapshot(DisplayMode target)
        {
            if (target == null || string.IsNullOrEmpty(target.DeviceName))
                return Failure("Choose one live display mode first.");

            LiveDisplayService.DevMode original = LiveDisplayService.CreateDevMode();
            if (!LiveDisplayService.EnumDisplaySettingsEx(target.DeviceName, EnumCurrentSettings, ref original, 0))
                return Failure("Unable to read the current display mode.");

            LiveDisplayService.DevMode requested = original;
            requested.PelsWidth = target.Width;
            requested.PelsHeight = target.Height;
            requested.DisplayFrequency = target.Frequency;
            requested.BitsPerPixel = target.BitsPerPixel;
            int testResult = ChangeDisplaySettingsEx(target.DeviceName, ref requested, IntPtr.Zero, CdsTest, IntPtr.Zero);
            if (testResult != DispChangeSuccessful)
                return Failure("Windows rejected this display mode during CDS_TEST. Return code: " + testResult + ".");

            DisplayModeSnapshot snapshot = new DisplayModeSnapshot
            {
                DeviceName = target.DeviceName,
                Width = original.PelsWidth,
                Height = original.PelsHeight,
                Frequency = original.DisplayFrequency,
                BitsPerPixel = original.BitsPerPixel,
                CreatedUtc = DateTime.UtcNow,
                Confirmed = false
            };
            AppPaths.EnsureWritableDataDirectories();
            _journals.Write(AppPaths.LatestDisplayModeSnapshotPath, snapshot);

            int applyResult = ChangeDisplaySettingsEx(target.DeviceName, ref requested, IntPtr.Zero, CdsUpdateRegistry, IntPtr.Zero);
            if (applyResult != DispChangeSuccessful)
            {
                TryRestore(snapshot);
                return Failure("Windows could not apply the display mode. Return code: " + applyResult + ".");
            }
            return new OperationResult { Succeeded = true, Message = "Display mode applied. Keep it within 15 seconds or it will be restored.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        internal OperationResult ConfirmPending()
        {
            if (!File.Exists(AppPaths.LatestDisplayModeSnapshotPath))
                return Failure("No pending display mode snapshot exists.");
            DisplayModeSnapshot snapshot = _journals.Read<DisplayModeSnapshot>(AppPaths.LatestDisplayModeSnapshotPath);
            snapshot.Confirmed = true;
            _journals.Write(AppPaths.LatestDisplayModeSnapshotPath, snapshot);
            return new OperationResult { Succeeded = true, Message = "The new current display mode was retained.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        internal OperationResult RestorePending()
        {
            if (!File.Exists(AppPaths.LatestDisplayModeSnapshotPath))
                return Failure("No pending display mode snapshot exists.");
            DisplayModeSnapshot snapshot = _journals.Read<DisplayModeSnapshot>(AppPaths.LatestDisplayModeSnapshotPath);
            bool restored = TryRestore(snapshot);
            return new OperationResult { Succeeded = restored, Message = restored ? "The original display mode was restored and verified." : "Windows could not restore the original display mode.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        private static bool TryRestore(DisplayModeSnapshot snapshot)
        {
            LiveDisplayService.DevMode value = LiveDisplayService.CreateDevMode();
            if (!LiveDisplayService.EnumDisplaySettingsEx(snapshot.DeviceName, EnumCurrentSettings, ref value, 0))
                return false;
            value.PelsWidth = snapshot.Width;
            value.PelsHeight = snapshot.Height;
            value.DisplayFrequency = snapshot.Frequency;
            value.BitsPerPixel = snapshot.BitsPerPixel;
            if (ChangeDisplaySettingsEx(snapshot.DeviceName, ref value, IntPtr.Zero, CdsTest, IntPtr.Zero) != DispChangeSuccessful)
                return false;
            if (ChangeDisplaySettingsEx(snapshot.DeviceName, ref value, IntPtr.Zero, CdsUpdateRegistry, IntPtr.Zero) != DispChangeSuccessful)
                return false;
            LiveDisplayService.DevMode verified = LiveDisplayService.CreateDevMode();
            return LiveDisplayService.EnumDisplaySettingsEx(snapshot.DeviceName, EnumCurrentSettings, ref verified, 0) && verified.PelsWidth == snapshot.Width && verified.PelsHeight == snapshot.Height;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern int ChangeDisplaySettingsEx(string deviceName, ref LiveDisplayService.DevMode devMode, IntPtr hwnd, int flags, IntPtr lParam);

        private static OperationResult Failure(string message)
        {
            return new OperationResult { Succeeded = false, Message = message };
        }
    }
}
