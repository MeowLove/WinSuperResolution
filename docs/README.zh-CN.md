# WinSuperResolution

[English](../README.md) | [简体中文](README.zh-CN.md) | [Русский](README.ru-RU.md)

适用于 Windows 的更高虚拟分辨率与类似 HiDPI 的缩放体验。

WinSuperResolution 用于配置更高的 Windows 虚拟桌面分辨率，并配合 Windows 显示缩放，在有限的物理分辨率下获得更大的工作空间和更好的可读性。

## 功能

- 扫描 Windows 中已注册的显示配置，包括历史配置。
- 根据活动信号分辨率或已注册的表面尺寸生成更高分辨率计划。
- 默认按单个显示配置应用，也支持经过确认的全部应用。
- 支持测试并应用当前桌面分辨率，包括 Windows 已验证的虚拟桌面模式。
- 提供带备份和恢复能力的实验性每显示器缩放。
- 支持 English、简体中文和 Русский。

本工具修改 Windows 显示配置，不是 AI 图像放大工具，也不提供 NVIDIA DLSS、AMD FSR、Intel XeSS 或同类 GPU 渲染功能。

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
5. 检查目标和提示后确认操作。
6. 调整桌面分辨率时，在模式列表中选择项目并点击“测试并应用桌面分辨率”。在倒计时结束前确认保留，否则程序会自动恢复原分辨率。
7. 调整实验性缩放时选择百分比并应用；如果程序提示，请注销或重启 Windows。

程序仍保留“显示设置”按钮，用户可以随时使用 Windows 自带设置手动调整。

## 便携数据

程序会将配置和恢复数据保存在 EXE 所在目录：

- `WinSuperResolution.settings.json`
- `backup_reg/`
- `backup_journal/`
- `display_state/`
- `logs/`

整体移动程序目录即可保留便携配置。

## 编译

使用 Visual Studio 打开 `WinSuperResolution.sln`，编译 `Release|x64` 配置。项目不依赖第三方 UI 框架或第三方运行库。

## 许可证

详见 [LICENSE](LICENSE)。
