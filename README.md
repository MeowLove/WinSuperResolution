# WinSuperResolution

Higher Virtual Resolution & HiDPI-Style Scaling for Windows.

WinSuperResolution is a native Windows desktop tool for managing registered display configurations, planning higher virtual desktop resolutions, and safely recovering configuration changes. The v2 rewrite targets Windows 11 24H2+ and uses WPF on the system-provided .NET Framework 4.8.1 runtime.

The application distinguishes the virtual-resolution capability stored in the display-configuration registry from the current Windows display mode and the current per-monitor scaling setting. It does not claim to provide AI image upscaling or to reproduce the macOS display stack.

## Development status

The repository currently contains the v2 implementation in progress. The legacy v1 source and binaries are preserved by the `legacy-v1.0-source` Git tag.
