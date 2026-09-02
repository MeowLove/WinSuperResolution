# WinSuperResolution

[English](../README.md) | [简体中文](README.zh-CN.md) | [Русский](README.ru-RU.md)

适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验。

WinSuperResolution 用于配置更高的 Windows 虚拟桌面分辨率，并配合 Windows 显示缩放，在有限的物理分辨率下获得更大的工作空间和更好的可读性。

## 关于作者

WinSuperResolution 由 **CXT** 创作，别名 **MeowLove**。

- 网站：[www.cxthhhhh.com](https://www.cxthhhhh.com/)
- V2 版本发布与介绍：[WinSuperResolution：Windows HiDPI 风格缩放 V2](https://www.cxthhhhh.com/2026/08/31/winsuperresolution-windows-hidpi-style-scaling-v2.html)

<img width="256" height="256" alt="WinSuperResolution_Logo" src="https://github.com/user-attachments/assets/33560a97-8cd2-40d8-ae23-6dce05fd663e" />

## 适用场景

WinSuperResolution 适用于以下场景：

- 在 1080p 级别显示器上使用更高的虚拟桌面分辨率，以显示更多桌面内容。
- 在 1440p（常称 2K）显示器上配置接近 4K 的虚拟桌面工作空间。
- 将虚拟分辨率与 Windows 缩放结合，获得更清晰、更宽裕的类似 HiDPI 体验。
- 当推荐的缩放比例不可用，或页面缩放后比例不协调时，改善界面尺寸与布局匹配。
- 在文字可读性和工作空间之间取得更好的平衡，减少界面过大或内容显示过少的问题。

典型示例：

- 1080p 显示器 → 约 1.5K 或更高的虚拟桌面分辨率。
- 1440p 显示器 → 约 4K 或更高的虚拟桌面分辨率。
- 4K 显示器 → 配合合适的 Windows 缩放比例，获得更大的虚拟工作空间。

通常的使用流程是：先配置虚拟分辨率能力，必要时重启或重新初始化显示栈，再选择当前 Windows 桌面分辨率，最后调整 Windows 缩放比例。对于支持 Windows DPI 缩放的应用，这有助于改善页面布局、文字可读性和工作空间平衡；它不会改变显示器的物理像素，也不能保证所有应用都得到完全相同的效果。

## 功能

- 扫描 Windows 中已注册的显示配置，包括历史配置。
- 根据活动信号分辨率或已注册的表面尺寸生成更高分辨率计划。
- 默认按单个显示配置应用，也支持经过确认的全部应用。
- 支持测试并应用当前桌面分辨率，包括 Windows 已验证的虚拟桌面模式。
- 提供带备份和恢复能力的实验性每显示器缩放。
- 支持 English、简体中文和 Русский。

本工具修改 Windows 显示配置，不是 AI 图像放大工具，也不提供 NVIDIA DLSS、AMD FSR、Intel XeSS 或同类 GPU 渲染功能。

<img width="3622" height="2395" alt="image" src="https://github.com/user-attachments/assets/9c5776b6-0102-416e-98a0-dd50def7b16a" />

## 系统要求

- Windows 11 24H2 或更高版本
- x64 系统
- .NET Framework 4.8.1
- 管理员权限

## 使用方法

1. 下载或编译 `WinSuperResolution.exe`。
2. 启动程序并通过管理员权限提示。
3. 在左侧列表选择显示配置。
4. 选择倍率并生成预览计划。
5. 检查目标和提示后，按需要使用“应用所选能力”或“全部应用能力”。
6. 如果程序提示，请重启 Windows 或重新初始化显示栈；然后在 Windows“显示设置”中选择预期的可用桌面分辨率。
7. 调整桌面分辨率时，在模式列表中选择项目并点击“测试并应用桌面分辨率”。在倒计时结束前确认保留，否则程序会自动恢复原分辨率。
8. 调整实验性缩放时选择百分比并应用；如果程序提示，请注销或重启 Windows。

程序仍保留“显示设置”按钮，用户可以随时使用 Windows 自带设置手动调整。

## 图像质量与兼容性

虚拟分辨率能力扩展的是 Windows 和显卡驱动可能提供的桌面模式集合。它不会增加面板的物理像素，不提供 GPU 渲染超采样，也不能保证每台设备、每个驱动或每个应用都提供特定模式或得到特定画面效果。

低分辨率显示器建议从 110% 开始，按 10% 增量测试。每次调整后，请选择可用的 Windows 桌面分辨率和阅读舒适的 Windows 显示缩放比例。更高倍率可能带来更多工作空间，但也可能使文字和界面变小，并暴露物理面板的限制。不存在通用的最大倍率；应在工作空间、清晰度和舒适度之间找到第一个可接受的平衡点。

## 排障与 GitHub Issues

如果某个活动显示器显示为“候选（Candidate）”或“配置冲突（Configuration conflict）”，或者程序列出的活动显示器数量多于实际连接的显示器，请先不要应用桌面分辨率、缩放或虚拟分辨率能力。列表行是注册表配置，不等于物理显示器数量；只有唯一稳定身份匹配才会成为 `Active + Exact`。未被取代的历史配置仍可单独用于生成虚拟分辨率能力计划。

### 设置未生效或之后被恢复时

1. 修改显示设置前，先退出可能干预显示会话的远程控制、远程游玩、云游戏、串流或显示器管理软件。ToDesk、TeamViewer，以及网易 UU 的相关模式等工具，可能创建或切换显示会话、虚拟显示器或缩放设置。
2. 如果显示器列表重复、始终为“候选（Candidate）”，或预期模式始终没有出现，请先导出诊断包。只有关联问题仍存在时，才使用“清理显示缓存（最终修复）”，并允许程序要求的 Windows 立即重启。
3. 重启后打开 WinSuperResolution，点击“刷新”，生成目标能力计划并检查全部目标。仅需修改一个目标配置时使用“应用所选能力”；只有确认列表中的每个目标都需要修改时，才使用“全部应用能力”。
4. 如果程序提示，请重启 Windows 或重新初始化显示驱动；然后在 Windows“显示设置”中选择预期的最高可用桌面分辨率。
5. 需要时注销或重启，然后选择所需的 Windows 显示缩放比例。
6. 重新连接远程控制或串流软件后，请重新检查已选分辨率和缩放。如果工具再次改变活动显示会话或配置，请重新执行经过检查的应用流程；反复出现时请导出新的诊断包。

提交问题前，请按以下步骤导出诊断包：

1. 重现问题，并记录“预期结果”和“实际结果”。
2. 在 WinSuperResolution 中点击“导出诊断包（Export Diagnostic Package）”。
3. 在程序 EXE 所在目录的 `diagnostics/` 下找到生成的 `WinSuperResolution-diagnostic-*.zip`。
4. 打开 [GitHub Issues](https://github.com/MeowLove/WinSuperResolution/issues)，写明复现步骤，并将 ZIP 文件拖入 Issue 表单作为附件。

请同时提供 WinSuperResolution 版本、Windows 版本/内部版本号、实际显示器数量、连接方式，以及重启 Windows 后问题是否仍然存在。已有诊断包时，不要只提交截图；诊断包包含排查显示器关联和注册表状态所需的结构化信息。

诊断包可能包含程序日志、操作日志、注册表导出、已有注册表备份、显示状态快照、程序设置、显示器标识和本地文件路径。上传前请检查 ZIP 内容，并删除或打码不希望公开的信息。诊断包只会在本地生成，程序不会自动上传。

如果导出诊断包后显示器关联问题仍然存在，最后再使用“清理显示缓存（最终修复）”。用户确认后，程序会先写入完整备份和 Journal，只有备份成功才删除 Windows 的 `GraphicsDrivers\\Configuration`、`Connectivity` 和 `ScaleFactors` 缓存，然后立即重启 Windows 让系统重新生成。操作前请保存工作；此操作会重置所有显示器的显示配置。清理失败或部分失败时不会自动重启。

## 便携数据

程序会将配置和恢复数据保存在 EXE 所在目录：

- `WinSuperResolution.settings.json`
- `backup_reg/`
- `backup_journal/`
- `display_state/`
- `logs/`
- `diagnostics/`

整体移动程序目录即可保留便携配置。

## 编译

要生成经过验证的本地发布包，请运行：

```powershell
.\scripts\Build-Release.ps1
.\scripts\Package-Release.ps1 -Version 2.3.0
```

编译脚本会使用 .NET Framework MSBuild 重建 `Release|x64`，并运行 Smoke Tests。打包脚本会从已验证的 EXE 和公开文档生成 `deliverables/WinSuperResolution-v<version>-win-x64.zip` 及其 SHA-256 文件；它不会上传 GitHub Release。

也可以使用 Visual Studio 打开 `WinSuperResolution.sln`，编译 `Release|x64` 配置。项目不依赖第三方 UI 框架或第三方运行库。

## 许可证

详见 [LICENSE](LICENSE)。
