# Game Overlay

一个轻量的 Windows 缓解晕3D工具。

它会在屏幕中心显示一组半透明参考线和中心准星，适合需要固定屏幕中心参考点的窗口、桌面应用、窗口化游戏和许多无边框全屏游戏场景。程序使用安全的透明置顶窗口实现，不注入游戏进程，也不 Hook DirectX。

## 功能特点

- 屏幕中心准星和四向参考线
- 托盘常驻，支持显示/隐藏和退出
- 快捷键切换显示状态：`Ctrl+Alt+X`
- 双击托盘图标打开设置
- 支持红、黄、蓝、绿、青五种颜色预设
- 支持调整透明度、线宽、中心间隙、准星长度和准星宽度
- 支持让四条参考线延伸到屏幕边缘
- 自动保存配置到用户目录，下次启动自动恢复

## 截图

暂无截图。运行程序后，托盘图标右键打开菜单，或双击托盘图标进入设置。

## 运行环境

- Windows
- 直接运行发布版：不需要额外安装 .NET Runtime
- 从源码运行或构建：需要 .NET Core 3.0 SDK

## 直接下载使用

下载发布包：

[GameOverlay-win-x64.zip](https://github.com/kt030415/game-overlay/raw/master/dist/GameOverlay-win-x64.zip)

仓库路径：

```text
dist\GameOverlay-win-x64.zip
```

解压后双击 `GameOverlay.exe` 即可运行，不需要安装 .NET Runtime。

## 从源码运行

```powershell
dotnet run --project src/GameOverlay.App/GameOverlay.App.csproj
```

程序启动后会出现在系统托盘中。如果当前配置为隐藏 overlay，程序会显示一个托盘提示。

## 构建发布版本

```powershell
dotnet publish src/GameOverlay.App/GameOverlay.App.csproj -c Release
```

发布配置会生成 Windows x64 自包含单文件版本。发布文件会生成到：

```text
src\GameOverlay.App\bin\Release\netcoreapp3.0\win-x64\publish
```

生成的 `GameOverlay.exe` 是自包含单文件程序。由于文件较大，建议压缩成 zip 后分发。

## 使用方式

- `Ctrl+Alt+X`：显示/隐藏 overlay
- 托盘图标右键：打开菜单
- 托盘图标双击：打开设置窗口
- 设置窗口中可以调整颜色、透明度、线宽、中心间隙和准星尺寸
- 退出程序前会自动保存当前配置

## 配置文件

配置文件保存在：

```text
%APPDATA%\GameOverlay\config.json
```

默认配置：

- 颜色：`#ffff00`
- 透明度：`0.35`
- 线宽：`28`
- 延伸到屏幕边缘：`true`
- 横线长度：`360`
- 竖线长度：`320`
- 中心间隙：`52`
- 准星长度：`34`
- 准星宽度：`6`
- 准星透明度：`0.9`

说明：横线长度和竖线长度保留在配置中，用于兼容旧配置和未延伸到边缘时的内部绘制逻辑；当前设置窗口不再显示这两个控制项。

## 全屏兼容性

本工具使用普通 Windows 透明置顶窗口实现，适合桌面窗口、窗口化游戏和很多无边框全屏游戏。

真正的独占全屏游戏可能会显示在 overlay 之上，这是 Windows 显示层级和游戏独占渲染方式决定的。为了降低兼容性和安全风险，本工具不会注入游戏进程，也不会修改游戏渲染管线。

## 开发与测试

运行测试：

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

构建 Debug 版本：

```powershell
dotnet build GameOverlay.sln -c Debug
```

## 许可

当前仓库暂未声明许可证。使用或分发前请自行确认授权范围。
