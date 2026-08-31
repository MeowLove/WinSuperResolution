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
        private readonly VirtualDesktopModeService _virtualDesktopModes;

        internal DisplayModeService(JournalService journals)
        {
            _journals = journals;
            _virtualDesktopModes = new VirtualDesktopModeService();
        }

        internal IList<DisplayMode> EnumerateModes(DisplayConfigurationRecord record)
        {
            List<DisplayMode> modes = new List<DisplayMode>();
            if (record == null || record.LiveDisplay == null || string.IsNullOrEmpty(record.LiveDisplay.DeviceName))
                return modes;

            string deviceName = record.LiveDisplay.DeviceName;
            DisplayMode currentDesktopMode;
            string virtualModeError;
            if (_virtualDesktopModes.TryGetCurrentDesktopMode(deviceName, out currentDesktopMode, out virtualModeError))
                modes.Add(currentDesktopMode);

            if (record.HasPrimarySurface && !ContainsResolution(modes, record.PrimarySurfaceWidth, record.PrimarySurfaceHeight, true))
            {
                modes.Add(new DisplayMode
                {
                    DeviceName = deviceName,
                    Width = record.PrimarySurfaceWidth,
                    Height = record.PrimarySurfaceHeight,
                    Frequency = currentDesktopMode == null ? 0 : currentDesktopMode.Frequency,
                    IsVirtualDesktopMode = true
                });
            }

            HashSet<string> seenDriverModes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                    BitsPerPixel = value.BitsPerPixel,
                    IsVirtualDesktopMode = false
                };
                string key = mode.Width + "x" + mode.Height + "@" + mode.Frequency + ":" + mode.BitsPerPixel;
                if (seenDriverModes.Add(key))
                    modes.Add(mode);
            }
            return modes;
        }

        internal OperationResult ApplyWithSnapshot(DisplayMode target)
        {
            if (target == null || string.IsNullOrEmpty(target.DeviceName))
                return Failure("Choose one desktop mode first.");

            if (target.IsVirtualDesktopMode)
                return ApplyVirtualDesktopMode(target);
            return ApplyDriverMode(target);
        }

        internal OperationResult ConfirmPending()
        {
            if (!File.Exists(AppPaths.LatestDisplayModeSnapshotPath))
                return Failure("No pending display mode snapshot exists.");
            DisplayModeSnapshot snapshot = _journals.Read<DisplayModeSnapshot>(AppPaths.LatestDisplayModeSnapshotPath);
            snapshot.Confirmed = true;
            _journals.Write(AppPaths.LatestDisplayModeSnapshotPath, snapshot);
            return new OperationResult { Succeeded = true, Message = "The new desktop mode was retained.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        internal OperationResult RestorePending()
        {
            if (!File.Exists(AppPaths.LatestDisplayModeSnapshotPath))
                return Failure("No pending display mode snapshot exists.");
            DisplayModeSnapshot snapshot = _journals.Read<DisplayModeSnapshot>(AppPaths.LatestDisplayModeSnapshotPath);
            bool restored = TryRestore(snapshot);
            return new OperationResult { Succeeded = restored, Message = restored ? "The original desktop mode was restored and verified." : "Windows could not restore the original desktop mode.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        private OperationResult ApplyVirtualDesktopMode(DisplayMode target)
        {
            DisplayModeSnapshot snapshot;
            string snapshotError;
            if (!TryCaptureSnapshot(target.DeviceName, out snapshot, out snapshotError))
                return Failure(snapshotError);

            AppPaths.EnsureWritableDataDirectories();
            _journals.Write(AppPaths.LatestDisplayModeSnapshotPath, snapshot);

            string applyError;
            if (!_virtualDesktopModes.TryApplyDesktopMode(target.DeviceName, target.Width, target.Height, out applyError))
            {
                TryRestore(snapshot);
                return Failure(applyError);
            }
            return Success();
        }

        private OperationResult ApplyDriverMode(DisplayMode target)
        {
            DisplayModeSnapshot snapshot;
            string snapshotError;
            if (!TryCaptureSnapshot(target.DeviceName, out snapshot, out snapshotError))
                return Failure(snapshotError);

            LiveDisplayService.DevMode original = LiveDisplayService.CreateDevMode();
            if (!LiveDisplayService.EnumDisplaySettingsEx(target.DeviceName, EnumCurrentSettings, ref original, 0))
                return Failure("Unable to read the current driver display mode.");

            LiveDisplayService.DevMode requested = original;
            requested.PelsWidth = target.Width;
            requested.PelsHeight = target.Height;
            requested.DisplayFrequency = target.Frequency;
            requested.BitsPerPixel = target.BitsPerPixel;
            int testResult = ChangeDisplaySettingsEx(target.DeviceName, ref requested, IntPtr.Zero, CdsTest, IntPtr.Zero);
            if (testResult != DispChangeSuccessful)
                return Failure("Windows rejected this driver display mode during CDS_TEST. Return code: " + testResult + ".");

            AppPaths.EnsureWritableDataDirectories();
            _journals.Write(AppPaths.LatestDisplayModeSnapshotPath, snapshot);
            int applyResult = ChangeDisplaySettingsEx(target.DeviceName, ref requested, IntPtr.Zero, CdsUpdateRegistry, IntPtr.Zero);
            if (applyResult != DispChangeSuccessful)
            {
                TryRestore(snapshot);
                return Failure("Windows could not apply the driver display mode. Return code: " + applyResult + ".");
            }
            return Success();
        }

        private bool TryCaptureSnapshot(string deviceName, out DisplayModeSnapshot snapshot, out string error)
        {
            snapshot = null;
            error = null;
            DisplayMode virtualDesktopMode;
            string virtualModeError;
            if (_virtualDesktopModes.TryGetCurrentDesktopMode(deviceName, out virtualDesktopMode, out virtualModeError))
            {
                snapshot = new DisplayModeSnapshot
                {
                    DeviceName = deviceName,
                    Width = virtualDesktopMode.Width,
                    Height = virtualDesktopMode.Height,
                    Frequency = virtualDesktopMode.Frequency,
                    BitsPerPixel = virtualDesktopMode.BitsPerPixel,
                    IsVirtualDesktopMode = true,
                    CreatedUtc = DateTime.UtcNow,
                    Confirmed = false
                };
                return true;
            }

            LiveDisplayService.DevMode original = LiveDisplayService.CreateDevMode();
            if (!LiveDisplayService.EnumDisplaySettingsEx(deviceName, EnumCurrentSettings, ref original, 0))
            {
                error = "Unable to read the current display mode. " + virtualModeError;
                return false;
            }
            snapshot = new DisplayModeSnapshot
            {
                DeviceName = deviceName,
                Width = original.PelsWidth,
                Height = original.PelsHeight,
                Frequency = original.DisplayFrequency,
                BitsPerPixel = original.BitsPerPixel,
                IsVirtualDesktopMode = false,
                CreatedUtc = DateTime.UtcNow,
                Confirmed = false
            };
            return true;
        }

        private bool TryRestore(DisplayModeSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.DeviceName))
                return false;
            if (snapshot.IsVirtualDesktopMode)
            {
                string virtualRestoreError;
                return _virtualDesktopModes.TryApplyDesktopMode(snapshot.DeviceName, snapshot.Width, snapshot.Height, out virtualRestoreError);
            }

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

        private static bool ContainsResolution(IList<DisplayMode> modes, int width, int height, bool virtualOnly)
        {
            foreach (DisplayMode mode in modes)
            {
                if (mode.Width == width && mode.Height == height && (!virtualOnly || mode.IsVirtualDesktopMode))
                    return true;
            }
            return false;
        }

        private static OperationResult Success()
        {
            return new OperationResult { Succeeded = true, Message = "Desktop mode applied. Keep it within 15 seconds or it will be restored.", JournalPath = AppPaths.LatestDisplayModeSnapshotPath };
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern int ChangeDisplaySettingsEx(string deviceName, ref LiveDisplayService.DevMode devMode, IntPtr hwnd, int flags, IntPtr lParam);

        private static OperationResult Failure(string message)
        {
            return new OperationResult { Succeeded = false, Message = message };
        }
    }
}
