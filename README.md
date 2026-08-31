# WinSuperResolution

[English](README.md) | [简体中文](docs/README.zh-CN.md) | [Русский](docs/README.ru-RU.md)

Higher Virtual Resolution & HiDPI-Style Scaling for Windows.

WinSuperResolution helps Windows users configure a higher virtual desktop resolution and combine it with Windows display scaling for a clearer, more spacious desktop experience similar in spirit to HiDPI.

## What It Does

- Scans registered display configurations, including historical entries.
- Plans higher virtual resolutions from the active signal or the registered surface size.
- Applies changes per display, with an optional reviewed batch operation.
- Lets you test and apply the current desktop resolution, including supported virtual desktop modes.
- Provides experimental per-monitor scaling with backups and recovery.
- Supports English, Simplified Chinese, and Russian.

This tool changes Windows display configuration. It is not an AI image upscaler and does not provide NVIDIA DLSS, AMD FSR, Intel XeSS, or equivalent GPU rendering features.

## Requirements

- Windows 11 24H2 or later
- x64 system
- .NET Framework 4.8.1
- Administrator permission

## Usage

1. Download or build `WinSuperResolution.exe`.
2. Run it and approve the administrator prompt.
3. Select a display configuration from the list.
4. Choose a magnification and generate a preview plan.
5. Review the targets and confirm the operation.
6. For desktop resolution, select a mode and use **Test & apply desktop mode**. Keep the new mode during the confirmation countdown; otherwise it is restored automatically.
7. For experimental scaling, select a percentage and apply it. Sign out or restart Windows if prompted.

The **Display Settings** button remains available for manual Windows configuration.

## Portable Data

The application stores its settings and recovery data beside the executable:

- `WinSuperResolution.settings.json`
- `backup_reg/`
- `backup_journal/`
- `display_state/`
- `logs/`

Move the executable directory as a whole to preserve the portable configuration.

## Build

Open `WinSuperResolution.sln` in Visual Studio and build the `Release|x64` configuration. The project uses the .NET Framework installed with Windows development tools and does not require third-party UI frameworks or runtimes.

## License

See [LICENSE](LICENSE).
