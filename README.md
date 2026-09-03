# WinSuperResolution

[English](README.md) | [简体中文](docs/README.zh-CN.md) | [Русский](docs/README.ru-RU.md)

Higher Virtual Resolution & HiDPI-Style Scaling for Windows.

WinSuperResolution helps Windows users configure a higher virtual desktop resolution and combine it with Windows display scaling for a clearer, more spacious desktop experience similar in spirit to HiDPI.

## About

WinSuperResolution is created by **CXT**, also known as **MeowLove**.

- Website: [www.cxthhhhh.com](https://www.cxthhhhh.com/)
- Version 2 release and introduction: [WinSuperResolution: Windows HiDPI-Style Scaling V2](https://www.cxthhhhh.com/2026/08/31/winsuperresolution-windows-hidpi-style-scaling-v2.html)

<img width="256" height="256" alt="WinSuperResolution_Logo" src="https://github.com/user-attachments/assets/33560a97-8cd2-40d8-ae23-6dce05fd663e" />

## Version 3.2.0

- Adds the **Environment and compatibility** panel above the registered display list. It reports the selected active display path, its exactly matched graphics adapter, driver version and driver date, plus other detected adapters in expandable evidence.
- Uses advisory-only red (**Unsupported**), yellow (**Experimental**), and green (**Can try**) states. These communicate observed support evidence and driver freshness; they never block an existing feature.
- Adds an in-app **About** window with the product version, author information, and separate links to the official website, introduction, and GitHub repository.
- Shows the assembly version in the main window title so diagnostic reports and user feedback can identify the running build.
- Shows a compact installed/not-installed status for NVIDIA Control Panel, AMD Software/Adrenalin, and Intel Arc Control, with details available in the compatibility evidence.
- Moves the Windows scaling advisory into the current desktop state panel and uses cautious wording because the exact result depends on the active display path and selected mode.
- Splits the main workspace into Home, Super Resolution, and Scaling tabs so lower-resolution screens can use each workflow without fitting every panel at once.

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


<img width="4320" height="2604" alt="image" src="https://github.com/user-attachments/assets/71262cdf-909c-4d5f-bf77-dc267ddf8afa" />


## Requirements

- Windows 11 24H2 or later
- x64 system
- .NET Framework 4.8.1
- Administrator permission

The operating-system baseline alone does not guarantee virtual-resolution support. The active Windows display path and graphics driver decide whether virtual desktop modes are available. The in-app **Environment and compatibility** panel reports the detected Windows version, CPU, graphics adapters, driver versions and dates, and the selected display path status. Its red, yellow, and green states are advisory only: they never block an operation.

## Usage

1. Download or build `WinSuperResolution.exe`.
2. Run it and approve the administrator prompt.
3. Select a display configuration from the list.
4. Choose a magnification and generate a preview plan.
5. Review the targets, then use **Apply selected capability** or **Apply all capabilities** as appropriate.
6. Restart Windows or reinitialize the display stack if prompted. In Windows **Display Settings**, choose the intended available desktop resolution.
7. For desktop resolution, select a mode and use **Test & apply desktop mode**. Keep the new mode during the confirmation countdown; otherwise it is restored automatically.
8. For experimental scaling, select a percentage and apply it. Sign out or restart Windows if prompted.

The **Display Settings** button remains available for manual Windows configuration.

## Image Quality and Compatibility

Virtual-resolution capability expands the desktop modes that Windows and the graphics driver may offer. It does not add physical panel pixels, provide GPU rendering supersampling, or guarantee a particular mode or visual result on every device, driver, or application.

A virtual-desktop path reported by Windows is the primary compatibility signal. Systems with an old graphics driver, an unsupported Windows 11 installation, or hardware outside Microsoft's current Windows 11 recommendations can still be inspected and tried, but mode availability and persistence after a restart are not guaranteed.

On a low-resolution display, start at 110% and test in 10% increments. After each change, choose an available Windows desktop mode and a Windows display scale that remains comfortable to read. Higher magnification can provide more workspace, but it can also make text and UI smaller and expose the limits of the physical panel. There is no universally correct maximum: stop at the first acceptable balance between workspace, clarity, and comfort.

## Troubleshooting and GitHub Issues

If an active display is shown as `Candidate` or `Configuration conflict`, or if WinSuperResolution lists more active displays than are physically connected, do not apply a desktop-mode, scaling, or virtual-capability change. Registry rows are configurations, not physical-display counts; only a unique stable identity match becomes `Active + Exact`. Historical, non-superseded rows may remain eligible for separate virtual-capability planning.

### When a change does not appear or is later reverted

1. Close remote-control, remote-play, cloud-gaming, streaming, or display-management software before changing display settings. Tools such as ToDesk, TeamViewer, or NetEase UU in applicable modes can create or switch display sessions, virtual displays, or scaling settings.
2. If the display list is duplicated, stays `Candidate`, or the expected mode never appears, first export a diagnostic package. Use **Reset display cache (final repair)** only when the association issue persists, then allow the required immediate Windows restart.
3. After restart, open WinSuperResolution, click **Refresh**, generate the intended capability plan, and review its targets. Use **Apply selected capability** for one intended configuration. Use **Apply all capabilities** only after confirming that every listed target should be changed.
4. Restart Windows or reinitialize the display driver if requested. In Windows **Display Settings**, choose the highest intended available desktop resolution.
5. Sign out or restart if needed, then select the desired Windows display scaling percentage.
6. When reconnecting a remote-control or streaming tool, recheck the selected resolution and scale. If the tool changes the active display session or configuration again, repeat the reviewed application flow; export a new diagnostic package if this repeats.

To report a problem:

1. Reproduce the problem and note what you expected and what happened.
2. In WinSuperResolution, click **Export Diagnostic Package**.
3. Find the generated ZIP beside the executable under `diagnostics/WinSuperResolution-diagnostic-*.zip`.
4. Open a [GitHub Issue](https://github.com/MeowLove/WinSuperResolution/issues), describe the steps to reproduce, and attach the ZIP by dragging it into the issue form.

Please include the WinSuperResolution version, Windows version/build, number of physical displays, connection type, and whether the issue persists after restarting Windows. Do not upload screenshots alone when a diagnostic package is available; the package contains the structured evidence needed to investigate display association and registry state.

Diagnostic packages may contain application logs, operation journals, registry exports, existing registry backups, display-state snapshots, application settings, monitor identifiers, and local file paths. Review the ZIP before uploading and redact or remove anything you do not want to share. The package is created locally and is not uploaded automatically by the application.

If the display-association problem persists after exporting a diagnostic package, use **Reset display cache (final repair)** only as a last resort. After your confirmation, the application writes backups and a Journal first, deletes the Windows `GraphicsDrivers\\Configuration`, `Connectivity`, and `ScaleFactors` caches only after the backups succeed, then immediately restarts Windows so it can rebuild them. Save your work first; this resets display configuration for all monitors. A failed or partial reset does not request an automatic restart.

## Portable Data

The application stores its settings and recovery data beside the executable:

- `WinSuperResolution.settings.json`
- `backup_reg/`
- `backup_journal/`
- `display_state/`
- `logs/`
- `diagnostics/`

Move the executable directory as a whole to preserve the portable configuration.

## Build

For a verified local release, run:

```powershell
.\scripts\Build-Release.ps1
.\scripts\Package-Release.ps1 -Version 3.2.0
```

The build script rebuilds `Release|x64` with .NET Framework MSBuild and runs the smoke tests. The package script creates `deliverables/WinSuperResolution-v<version>-win-x64.zip` and its SHA-256 file from the verified executable and public documentation. It does not upload a GitHub Release.

You can also open `WinSuperResolution.sln` in Visual Studio and build the `Release|x64` configuration. The project uses the .NET Framework installed with Windows development tools and does not require third-party UI frameworks or runtimes.

## License

See [LICENSE](LICENSE).
