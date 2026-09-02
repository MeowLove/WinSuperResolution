# WinSuperResolution v2.3.0

WinSuperResolution v2.3 improves display-association safety, diagnostics, recovery, and product information for Windows virtual-resolution workflows.

## Highlights

- Improved matching between Windows active displays and registered display configurations.
- Prevents unsafe changes when multiple registry configurations map to the same active display.
- Keeps ambiguous or candidate display associations read-only to avoid changing the wrong configuration.
- Added **Export Diagnostic Package** for collecting logs, journals, display-state snapshots, registry exports, existing backups, and relevant application settings.
- Added **Reset display cache (final repair)** for persistent duplicated-display or association problems. The operation creates backups and a Journal before clearing Windows display caches, then requires an immediate restart.
- Added an in-app **About CXT (MeowLove)** entry that opens the official Version 2 release and introduction article.
- Improved recovery guidance for display changes that do not appear after application or are later overwritten.
- Added guidance for remote-control, streaming, cloud-gaming, and display-management software that may switch display sessions, create virtual displays, or overwrite resolution and scaling settings.
- Improved documentation for choosing virtual-resolution magnification and Windows display scaling.

## About

WinSuperResolution is created by **CXT**, also known as **MeowLove**.

- Website: [www.cxthhhhh.com](https://www.cxthhhhh.com/)
- Version 2 release and introduction: [WinSuperResolution: Windows HiDPI-Style Scaling V2](https://www.cxthhhhh.com/2026/08/31/winsuperresolution-windows-hidpi-style-scaling-v2.html)

## Important Notes

- Virtual-resolution capability changes the set of desktop modes that Windows and the graphics driver may offer. It does not add physical panel pixels or provide GPU rendering supersampling.
- After applying a capability plan, restart Windows or reinitialize the display stack if requested, then choose the intended available desktop resolution in Windows Display Settings.
- On lower-resolution displays, start at 110% and test in 10% increments. Choose the first comfortable balance between workspace, clarity, and readability.
- Registry configuration rows are not physical-display counts. If a display is shown as `Candidate` or `Configuration conflict`, do not apply display-mode, scaling, or virtual-capability changes.
- If a problem persists, export a diagnostic package and attach it to a GitHub Issue.

## Requirements

- Windows 11 24H2 or later
- x64 system
- Administrator permission

## Upgrade

Replace the previous `WinSuperResolution.exe` with the v2.3.0 executable. Keep the existing application directory if you want to retain local settings, backups, journals, and diagnostic history.
