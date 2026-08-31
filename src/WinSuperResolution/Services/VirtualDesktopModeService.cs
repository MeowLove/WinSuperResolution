using System;
using System.Runtime.InteropServices;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class VirtualDesktopModeService
    {
        private const int ErrorSuccess = 0;
        private const int ErrorInsufficientBuffer = 122;
        private const uint QueryOnlyActivePaths = 0x00000002;
        private const uint QueryVirtualModeAware = 0x00000010;
        private const uint SetUseSuppliedDisplayConfig = 0x00000020;
        private const uint SetValidate = 0x00000040;
        private const uint SetApply = 0x00000080;
        private const uint SetSaveToDatabase = 0x00000200;
        private const uint SetAllowChanges = 0x00000400;
        private const uint SetVirtualModeAware = 0x00008000;
        private const uint DisplayConfigPathSupportVirtualMode = 0x00000008;
        private const uint DisplayConfigModeInfoTypeSource = 1;
        private const uint DisplayConfigModeInfoTypeDesktopImage = 3;
        private const uint InvalidModeInfoIndex = 0x0000FFFF;
        private const uint DisplayConfigDeviceInfoGetSourceName = 1;

        internal bool TryGetCurrentDesktopMode(string deviceName, out DisplayMode mode, out string error)
        {
            mode = null;
            error = null;
            Topology topology;
            if (!TryQueryActiveTopology(out topology, out error))
                return false;

            int pathIndex = FindPathIndex(topology, deviceName);
            if (pathIndex < 0)
            {
                error = "The active display path was not found.";
                return false;
            }

            DisplayConfigPathInfo path = topology.Paths[pathIndex];
            if ((path.Flags & DisplayConfigPathSupportVirtualMode) == 0)
            {
                error = "The active display path does not report virtual-desktop support.";
                return false;
            }

            uint sourceModeIndex = GetVirtualSourceModeInfoIndex(path.SourceInfo.ModeInfoIdx);
            if (!IsExpectedModeInfo(topology, sourceModeIndex, DisplayConfigModeInfoTypeSource))
            {
                error = "The active display path does not expose a virtual desktop source mode.";
                return false;
            }

            DisplayConfigSourceMode source = topology.Modes[sourceModeIndex].ModeInfo.SourceMode;
            if (source.Width == 0 || source.Height == 0)
            {
                error = "The virtual desktop source mode has no usable resolution.";
                return false;
            }

            mode = new DisplayMode
            {
                DeviceName = deviceName,
                Width = (int)source.Width,
                Height = (int)source.Height,
                Frequency = ToFrequency(path.TargetInfo.RefreshRate),
                IsVirtualDesktopMode = true,
                IsCurrent = true
            };
            return true;
        }

        internal bool TryApplyDesktopMode(string deviceName, int width, int height, out string error)
        {
            error = null;
            if (width <= 0 || height <= 0)
            {
                error = "The requested desktop resolution is invalid.";
                return false;
            }

            Topology topology;
            if (!TryQueryActiveTopology(out topology, out error))
                return false;

            int selectedPathIndex = FindPathIndex(topology, deviceName);
            if (selectedPathIndex < 0)
            {
                error = "The active display path was not found.";
                return false;
            }

            DisplayConfigPathSourceInfo selectedSource = topology.Paths[selectedPathIndex].SourceInfo;
            if ((topology.Paths[selectedPathIndex].Flags & DisplayConfigPathSupportVirtualMode) == 0)
            {
                error = "The active display path does not support virtual desktop updates.";
                return false;
            }

            bool changed = false;
            for (int index = 0; index < topology.Paths.Length; index++)
            {
                DisplayConfigPathInfo path = topology.Paths[index];
                if (!SameSource(path.SourceInfo, selectedSource))
                    continue;
                if ((path.Flags & DisplayConfigPathSupportVirtualMode) == 0)
                {
                    error = "A cloned active display path does not support virtual desktop updates.";
                    return false;
                }

                uint sourceModeIndex = GetVirtualSourceModeInfoIndex(path.SourceInfo.ModeInfoIdx);
                uint desktopImageIndex = GetVirtualDesktopImageModeInfoIndex(path.TargetInfo.ModeInfoIdx);
                if (!IsExpectedModeInfo(topology, sourceModeIndex, DisplayConfigModeInfoTypeSource) ||
                    !IsExpectedModeInfo(topology, desktopImageIndex, DisplayConfigModeInfoTypeDesktopImage))
                {
                    error = "The active display path does not expose the virtual desktop mode records required for this change.";
                    return false;
                }

                DisplayConfigModeInfo sourceMode = topology.Modes[sourceModeIndex];
                sourceMode.ModeInfo.SourceMode.Width = (uint)width;
                sourceMode.ModeInfo.SourceMode.Height = (uint)height;
                topology.Modes[sourceModeIndex] = sourceMode;

                DisplayConfigModeInfo desktopMode = topology.Modes[desktopImageIndex];
                DisplayConfigDesktopImageInfo desktop = desktopMode.ModeInfo.DesktopImage;
                desktop.PathSourceSize.X = width;
                desktop.PathSourceSize.Y = height;
                desktop.DesktopImageRegion.Right = desktop.DesktopImageRegion.Left + width;
                desktop.DesktopImageRegion.Bottom = desktop.DesktopImageRegion.Top + height;
                desktop.DesktopImageClip.Right = desktop.DesktopImageClip.Left + width;
                desktop.DesktopImageClip.Bottom = desktop.DesktopImageClip.Top + height;
                desktopMode.ModeInfo.DesktopImage = desktop;
                topology.Modes[desktopImageIndex] = desktopMode;
                changed = true;
            }

            if (!changed)
            {
                error = "No virtual desktop path was available for the selected display.";
                return false;
            }

            uint validateFlags = SetUseSuppliedDisplayConfig | SetValidate | SetAllowChanges | SetVirtualModeAware;
            int validationResult = SetDisplayConfig((uint)topology.Paths.Length, topology.Paths, (uint)topology.Modes.Length, topology.Modes, validateFlags);
            if (validationResult != ErrorSuccess)
            {
                error = "Windows rejected the requested virtual desktop mode during validation. Return code: " + validationResult + ".";
                return false;
            }

            uint applyFlags = SetUseSuppliedDisplayConfig | SetApply | SetSaveToDatabase | SetAllowChanges | SetVirtualModeAware;
            int applyResult = SetDisplayConfig((uint)topology.Paths.Length, topology.Paths, (uint)topology.Modes.Length, topology.Modes, applyFlags);
            if (applyResult != ErrorSuccess)
            {
                error = "Windows could not apply the requested virtual desktop mode. Return code: " + applyResult + ".";
                return false;
            }

            DisplayMode verified;
            string verificationError;
            if (!TryGetCurrentDesktopMode(deviceName, out verified, out verificationError) || verified.Width != width || verified.Height != height)
            {
                error = "Windows applied the display configuration, but the requested desktop resolution could not be verified. " + verificationError;
                return false;
            }
            return true;
        }

        internal static uint GetVirtualSourceModeInfoIndex(uint modeInfoIndex)
        {
            return (modeInfoIndex >> 16) & InvalidModeInfoIndex;
        }

        internal static uint GetVirtualDesktopImageModeInfoIndex(uint modeInfoIndex)
        {
            return modeInfoIndex & InvalidModeInfoIndex;
        }

        private static bool TryQueryActiveTopology(out Topology topology, out string error)
        {
            topology = null;
            error = null;
            const uint queryFlags = QueryOnlyActivePaths | QueryVirtualModeAware;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                uint pathCount;
                uint modeCount;
                int sizeResult = GetDisplayConfigBufferSizes(queryFlags, out pathCount, out modeCount);
                if (sizeResult != ErrorSuccess)
                {
                    error = "Windows could not query the active display configuration. Return code: " + sizeResult + ".";
                    return false;
                }
                if (pathCount == 0)
                {
                    error = "Windows reported no active display paths.";
                    return false;
                }

                DisplayConfigPathInfo[] paths = new DisplayConfigPathInfo[pathCount];
                DisplayConfigModeInfo[] modes = new DisplayConfigModeInfo[modeCount];
                uint suppliedPathCount = pathCount;
                uint suppliedModeCount = modeCount;
                int queryResult = QueryDisplayConfig(queryFlags, ref suppliedPathCount, paths, ref suppliedModeCount, modes, IntPtr.Zero);
                if (queryResult == ErrorInsufficientBuffer)
                    continue;
                if (queryResult != ErrorSuccess)
                {
                    error = "Windows could not read the active display configuration. Return code: " + queryResult + ".";
                    return false;
                }

                if (suppliedPathCount != paths.Length)
                    Array.Resize(ref paths, (int)suppliedPathCount);
                if (suppliedModeCount != modes.Length)
                    Array.Resize(ref modes, (int)suppliedModeCount);
                topology = new Topology(paths, modes);
                return true;
            }

            error = "The active display configuration changed while it was being read. Please refresh and try again.";
            return false;
        }

        private static int FindPathIndex(Topology topology, string deviceName)
        {
            for (int index = 0; index < topology.Paths.Length; index++)
            {
                DisplayConfigSourceDeviceName sourceName = new DisplayConfigSourceDeviceName();
                sourceName.Header.Type = DisplayConfigDeviceInfoGetSourceName;
                sourceName.Header.Size = (uint)Marshal.SizeOf(typeof(DisplayConfigSourceDeviceName));
                sourceName.Header.AdapterId = topology.Paths[index].SourceInfo.AdapterId;
                sourceName.Header.Id = topology.Paths[index].SourceInfo.Id;
                if (DisplayConfigGetDeviceInfo(ref sourceName) != ErrorSuccess)
                    continue;
                if (string.Equals(sourceName.ViewGdiDeviceName, deviceName, StringComparison.OrdinalIgnoreCase))
                    return index;
            }
            return -1;
        }

        private static bool IsExpectedModeInfo(Topology topology, uint index, uint expectedType)
        {
            return index != InvalidModeInfoIndex && index < (uint)topology.Modes.Length && topology.Modes[index].InfoType == expectedType;
        }

        private static bool SameSource(DisplayConfigPathSourceInfo first, DisplayConfigPathSourceInfo second)
        {
            return first.AdapterId.LowPart == second.AdapterId.LowPart && first.AdapterId.HighPart == second.AdapterId.HighPart && first.Id == second.Id;
        }

        private static int ToFrequency(Rational refreshRate)
        {
            return refreshRate.Denominator == 0 ? 0 : (int)Math.Round((double)refreshRate.Numerator / refreshRate.Denominator);
        }

        private sealed class Topology
        {
            internal Topology(DisplayConfigPathInfo[] paths, DisplayConfigModeInfo[] modes)
            {
                Paths = paths;
                Modes = modes;
            }

            internal DisplayConfigPathInfo[] Paths { get; private set; }
            internal DisplayConfigModeInfo[] Modes { get; private set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Luid
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rational
        {
            public uint Numerator;
            public uint Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SizeL
        {
            public uint Width;
            public uint Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PointL
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RectL
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigPathSourceInfo
        {
            public Luid AdapterId;
            public uint Id;
            public uint ModeInfoIdx;
            public uint StatusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigPathTargetInfo
        {
            public Luid AdapterId;
            public uint Id;
            public uint ModeInfoIdx;
            public uint OutputTechnology;
            public uint Rotation;
            public uint Scaling;
            public Rational RefreshRate;
            public uint ScanLineOrdering;
            public uint TargetAvailable;
            public uint StatusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigPathInfo
        {
            public DisplayConfigPathSourceInfo SourceInfo;
            public DisplayConfigPathTargetInfo TargetInfo;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigSourceMode
        {
            public uint Width;
            public uint Height;
            public uint PixelFormat;
            public PointL Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigTargetMode
        {
            public ulong PixelRate;
            public Rational HSyncFrequency;
            public Rational VSyncFrequency;
            public SizeL ActiveSize;
            public SizeL TotalSize;
            public uint VideoStandardOrAdditionalSignalInfo;
            public uint ScanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigDesktopImageInfo
        {
            public PointL PathSourceSize;
            public RectL DesktopImageRegion;
            public RectL DesktopImageClip;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DisplayConfigModeInfoUnion
        {
            [FieldOffset(0)] public DisplayConfigSourceMode SourceMode;
            [FieldOffset(0)] public DisplayConfigTargetMode TargetMode;
            [FieldOffset(0)] public DisplayConfigDesktopImageInfo DesktopImage;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigModeInfo
        {
            public uint InfoType;
            public uint Id;
            public Luid AdapterId;
            public DisplayConfigModeInfoUnion ModeInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigDeviceInfoHeader
        {
            public uint Type;
            public uint Size;
            public Luid AdapterId;
            public uint Id;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayConfigSourceDeviceName
        {
            public DisplayConfigDeviceInfoHeader Header;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string ViewGdiDeviceName;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int QueryDisplayConfig(uint flags, ref uint pathCount, [Out] DisplayConfigPathInfo[] paths, ref uint modeCount, [Out] DisplayConfigModeInfo[] modes, IntPtr currentTopologyId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetDisplayConfig(uint pathCount, [In, Out] DisplayConfigPathInfo[] paths, uint modeCount, [In, Out] DisplayConfigModeInfo[] modes, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigSourceDeviceName requestPacket);
    }
}
