# WinSuperResolution

[English](README.md) | [简体中文](docs/README.zh-CN.md) | [Русский](docs/README.ru-RU.md)

Higher Virtual Resolution & HiDPI-Style Scaling for Windows.

WinSuperResolution helps Windows users configure a higher virtual desktop resolution and combine it with Windows display scaling for a clearer, more spacious desktop experience similar in spirit to HiDPI.

<img width="256" height="256" alt="WinSuperResolution_Logo" src="https://github.com/user-attachments/assets/33560a97-8cd2-40d8-ae23-6dce05fd663e" />

## Use Cases

WinSuperResolution is designed for the following scenarios:

- Use a higher virtual desktop resolution on 1080p-class displays to show more desktop content.
- Use a higher virtual desktop resolution on 1440p-class displays to approach a 4K-style workspace.
- Combine virtual resolution with Windows scaling to create a clearer, more spacious HiDPI-Style experience.
- Improve layout and UI-size matching when the preferred scaling percentage is unavailable or produces an unsatisfactory result.
- Balance readable text with available workspace instead of accepting an oversized or cramped interface.

Typical examples include:

- 1080p display → approximately 1.5K or higher virtual desktop resolution.
- 1440p display → approximately 4K or higher virtual desktop resolution.
- 4K display → a larger virtual workspace with an appropriate Windows scaling percentage.

The usual workflow is: configure virtual-resolution capability, restart or reinitialize the display stack when required, choose the current Windows desktop mode, and then adjust Windows scaling. This improves layout, text readability, and workspace balance where applications support Windows DPI scaling; it does not change the panel's physical pixels or guarantee identical results in every application.

## What It Does

- Scans registered display configurations, including historical entries.
- Plans higher virtual resolutions from the active signal or the registered surface size.
- Applies changes per display, with an optional reviewed batch operation.
- Lets you test and apply the current desktop resolution, including supported virtual desktop modes.
- Provides experimental per-monitor scaling with backups and recovery.
- Supports English, Simplified Chinese, and Russian.

This tool changes Windows display configuration. It is not an AI image upscaler and does not provide NVIDIA DLSS, AMD FSR, Intel XeSS, or equivalent GPU rendering features.

<img width="4320" height="2598" alt="Demo" src="https://github.com/user-attachments/assets/0b4a9972-723b-4125-aac2-e5680bcd9ad4" />


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

## Troubleshooting and GitHub Issues

If a display is shown as `Candidate` or `Configuration conflict`, or if WinSuperResolution lists more active displays than are physically connected, do not apply a virtual-resolution or scaling change. The application keeps these associations read-only until the registry configuration can be identified safely.

To report a problem:

1. Reproduce the problem and note what you expected and what happened.
2. In WinSuperResolution, click **Export Diagnostic Package**.
3. Find the generated ZIP beside the executable under `diagnostics/WinSuperResolution-diagnostic-*.zip`.
4. Open a [GitHub Issue](https://github.com/MeowLove/WinSuperResolution/issues), describe the steps to reproduce, and attach the ZIP by dragging it into the issue form.

Please include the WinSuperResolution version, Windows version/build, number of physical displays, connection type, and whether the issue persists after restarting Windows. Do not upload screenshots alone when a diagnostic package is available; the package contains the structured evidence needed to investigate display association and registry state.

Diagnostic packages may contain application logs, operation journals, registry exports, existing registry backups, display-state snapshots, application settings, monitor identifiers, and local file paths. Review the ZIP before uploading and redact or remove anything you do not want to share. The package is created locally and is not uploaded automatically by the application.

If the display-association problem persists after exporting a diagnostic package, use **Reset display cache (final repair)** only as a last resort. The application backs up and deletes the Windows \`GraphicsDrivers\\Configuration\`, \`Connectivity\`, and \`ScaleFactors\` caches, then immediately restarts Windows so it can rebuild them. Save your work first; this resets display configuration for all monitors.

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
