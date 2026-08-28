using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class LiveDisplayService
    {
        private const int EnumCurrentSettings = -1;
        private const int DisplayDeviceAttachedToDesktop = 0x00000001;
        private const int DpiTypeEffective = 0;
        private const uint QdcOnlyActivePaths = 0x00000002;
        private const uint DisplayConfigDeviceInfoGetSourceName = 1;
        private const uint DisplayConfigDeviceInfoGetTargetName = 2;

        internal IList<LiveDisplayInfo> Enumerate()
        {
            Dictionary<string, int> dpiByDevice = EnumerateEffectiveDpi();
            IDictionary<string, DisplayTargetDetails> activePathDevices = QueryActivePathDetails();
            List<LiveDisplayInfo> displays = new List<LiveDisplayInfo>();
            uint index = 0;
            while (true)
            {
                DisplayDevice adapter = CreateDisplayDevice();
                if (!EnumDisplayDevices(null, index, ref adapter, 0))
                    break;
                index++;
                if ((adapter.StateFlags & DisplayDeviceAttachedToDesktop) == 0)
                    continue;

                DisplayDevice monitor = CreateDisplayDevice();
                bool hasMonitor = EnumDisplayDevices(adapter.DeviceName, 0, ref monitor, 0);
                DevMode mode = CreateDevMode();
                bool hasMode = EnumDisplaySettingsEx(adapter.DeviceName, EnumCurrentSettings, ref mode, 0);
                int dpi;
                dpiByDevice.TryGetValue(adapter.DeviceName, out dpi);
                DisplayTargetDetails targetDetails;
                activePathDevices.TryGetValue(adapter.DeviceName, out targetDetails);
                displays.Add(new LiveDisplayInfo
                {
                    DeviceName = adapter.DeviceName,
                    AdapterName = adapter.DeviceString,
                    FriendlyName = hasMonitor && !string.IsNullOrEmpty(monitor.DeviceString) ? monitor.DeviceString : adapter.DeviceString,
                    MonitorDeviceId = hasMonitor ? monitor.DeviceId : string.Empty,
                    MonitorDeviceKey = hasMonitor ? monitor.DeviceKey : string.Empty,
                    ConnectionTechnology = targetDetails == null ? "Unknown" : DescribeOutputTechnology(targetDetails.OutputTechnology),
                    EdidManufacturer = targetDetails == null ? string.Empty : DecodeEdidManufacturer(targetDetails.EdidManufacturerId),
                    EdidProductCode = targetDetails == null ? 0 : targetDetails.EdidProductCodeId,
                    MonitorDevicePath = targetDetails == null ? string.Empty : targetDetails.MonitorDevicePath,
                    TopologyEvidence = targetDetails == null ? "EnumDisplayDevices active desktop" : "QueryDisplayConfig target + EnumDisplayDevices",
                    CurrentWidth = hasMode ? (int)mode.PelsWidth : 0,
                    CurrentHeight = hasMode ? (int)mode.PelsHeight : 0,
                    CurrentScalePercent = dpi > 0 ? (int)Math.Round(dpi * 100.0 / 96.0) : 0,
                    IsAttachedToDesktop = true
                });
            }
            return displays;
        }

        private static IDictionary<string, DisplayTargetDetails> QueryActivePathDetails()
        {
            Dictionary<string, DisplayTargetDetails> values = new Dictionary<string, DisplayTargetDetails>(StringComparer.OrdinalIgnoreCase);
            uint pathCount;
            uint modeCount;
            if (GetDisplayConfigBufferSizes(QdcOnlyActivePaths, out pathCount, out modeCount) != 0 || pathCount == 0)
                return values;
            DisplayConfigPathInfo[] paths = new DisplayConfigPathInfo[pathCount];
            DisplayConfigModeInfo[] modes = new DisplayConfigModeInfo[modeCount];
            uint suppliedPathCount = pathCount;
            uint suppliedModeCount = modeCount;
            if (QueryDisplayConfig(QdcOnlyActivePaths, ref suppliedPathCount, paths, ref suppliedModeCount, modes, IntPtr.Zero) != 0)
                return values;
            for (int index = 0; index < suppliedPathCount; index++)
            {
                DisplayConfigSourceDeviceName sourceName = new DisplayConfigSourceDeviceName();
                sourceName.Header.Type = DisplayConfigDeviceInfoGetSourceName;
                sourceName.Header.Size = (uint)Marshal.SizeOf(typeof(DisplayConfigSourceDeviceName));
                sourceName.Header.AdapterId = paths[index].SourceInfo.AdapterId;
                sourceName.Header.Id = paths[index].SourceInfo.Id;
                if (DisplayConfigGetDeviceInfo(ref sourceName) != 0 || string.IsNullOrEmpty(sourceName.ViewGdiDeviceName))
                    continue;
                DisplayConfigTargetDeviceName targetName = new DisplayConfigTargetDeviceName();
                targetName.Header.Type = DisplayConfigDeviceInfoGetTargetName;
                targetName.Header.Size = (uint)Marshal.SizeOf(typeof(DisplayConfigTargetDeviceName));
                targetName.Header.AdapterId = paths[index].TargetInfo.AdapterId;
                targetName.Header.Id = paths[index].TargetInfo.Id;
                if (DisplayConfigGetDeviceInfo(ref targetName) != 0)
                    continue;
                values[sourceName.ViewGdiDeviceName] = new DisplayTargetDetails
                {
                    OutputTechnology = targetName.OutputTechnology,
                    EdidManufacturerId = targetName.EdidManufacturerId,
                    EdidProductCodeId = targetName.EdidProductCodeId,
                    MonitorDevicePath = targetName.MonitorDevicePath
                };
            }
            return values;
        }

        private static string DecodeEdidManufacturer(ushort value)
        {
            if (value == 0)
                return string.Empty;
            char first = (char)(((value >> 10) & 0x1F) + 64);
            char second = (char)(((value >> 5) & 0x1F) + 64);
            char third = (char)((value & 0x1F) + 64);
            return first + second.ToString() + third;
        }

        private static string DescribeOutputTechnology(uint value)
        {
            switch (value)
            {
                case 5: return "HDMI";
                case 10: return "DisplayPort";
                case 11: return "eDP";
                case 0x80000000: return "Internal";
                default: return "DisplayConfig:" + value;
            }
        }

        private static Dictionary<string, int> EnumerateEffectiveDpi()
        {
            Dictionary<string, int> values = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            MonitorEnumProc callback = delegate(IntPtr monitor, IntPtr hdc, IntPtr rectangle, IntPtr data)
            {
                MonitorInfoEx info = new MonitorInfoEx();
                info.CbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
                if (GetMonitorInfo(monitor, ref info))
                {
                    uint dpiX;
                    uint dpiY;
                    try
                    {
                        if (GetDpiForMonitor(monitor, DpiTypeEffective, out dpiX, out dpiY) == 0 && dpiX > 0)
                            values[info.DeviceName] = (int)dpiX;
                    }
                    catch (DllNotFoundException)
                    {
                    }
                    catch (EntryPointNotFoundException)
                    {
                    }
                }
                return true;
            };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return values;
        }

        private static DisplayDevice CreateDisplayDevice()
        {
            DisplayDevice value = new DisplayDevice();
            value.Cb = Marshal.SizeOf(typeof(DisplayDevice));
            return value;
        }

        internal static DevMode CreateDevMode()
        {
            DevMode value = new DevMode();
            value.DmSize = (short)Marshal.SizeOf(typeof(DevMode));
            return value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DisplayDevice
        {
            public int Cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DevMode
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
            public short SpecVersion;
            public short DriverVersion;
            public short DmSize;
            public short DriverExtra;
            public int Fields;
            public int PositionX;
            public int PositionY;
            public int DisplayOrientation;
            public int DisplayFixedOutput;
            public short Color;
            public short Duplex;
            public short YResolution;
            public short TTOption;
            public short Collate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string FormName;
            public short LogPixels;
            public int BitsPerPixel;
            public int PelsWidth;
            public int PelsHeight;
            public int DisplayFlags;
            public int DisplayFrequency;
            public int IcmMethod;
            public int IcmIntent;
            public int MediaType;
            public int DitherType;
            public int Reserved1;
            public int Reserved2;
            public int PanningWidth;
            public int PanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MonitorInfoEx
        {
            public int CbSize;
            public Rect Monitor;
            public Rect Work;
            public int Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr rectangle, IntPtr data);

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
            public int PositionX;
            public int PositionY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayConfigTargetMode
        {
            public ulong PixelRate;
            public Rational HSyncFrequency;
            public Rational VSyncFrequency;
            public uint ActiveWidth;
            public uint ActiveHeight;
            public uint TotalWidth;
            public uint TotalHeight;
            public uint ScanLineOrdering;
            public uint VideoStandard;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DisplayConfigModeInfoUnion
        {
            [FieldOffset(0)] public DisplayConfigSourceMode SourceMode;
            [FieldOffset(0)] public DisplayConfigTargetMode TargetMode;
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayConfigTargetDeviceName
        {
            public DisplayConfigDeviceInfoHeader Header;
            public uint Flags;
            public uint OutputTechnology;
            public ushort EdidManufacturerId;
            public ushort EdidProductCodeId;
            public uint ConnectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string MonitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string MonitorDevicePath;
        }

        private sealed class DisplayTargetDetails
        {
            internal uint OutputTechnology { get; set; }
            internal ushort EdidManufacturerId { get; set; }
            internal ushort EdidProductCodeId { get; set; }
            internal string MonitorDevicePath { get; set; }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumDisplayDevices(string device, uint deviceIndex, ref DisplayDevice displayDevice, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EnumDisplaySettingsEx(string deviceName, int modeNum, ref DevMode devMode, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clipRect, MonitorEnumProc callback, IntPtr data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfoEx monitorInfo);

        [DllImport("shcore.dll", SetLastError = true)]
        private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int QueryDisplayConfig(uint flags, ref uint pathCount, [Out] DisplayConfigPathInfo[] paths, ref uint modeCount, [Out] DisplayConfigModeInfo[] modes, IntPtr currentTopologyId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigSourceDeviceName requestPacket);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigTargetDeviceName requestPacket);
    }
}
