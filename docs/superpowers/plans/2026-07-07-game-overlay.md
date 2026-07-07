# Game Overlay Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Windows tray utility that toggles a centered translucent yellow overlay above supported windows and games.

**Architecture:** Use a small .NET desktop application with a WPF transparent overlay window for real per-pixel opacity, plus WinForms `NotifyIcon` for tray integration. Keep testable geometry and configuration code in a core class library, and keep the app project focused on OS integration and drawing.

**Tech Stack:** .NET Core SDK 3.0, C# 8, WPF, WinForms tray icon, Win32 P/Invoke, `System.Text.Json`, no third-party dependencies.

---

## File Structure

- `GameOverlay.sln`: solution file.
- `src/GameOverlay.Core/GameOverlay.Core.csproj`: testable configuration and geometry library.
- `src/GameOverlay.Core/OverlayConfig.cs`: strongly typed overlay settings and defaults.
- `src/GameOverlay.Core/ConfigStore.cs`: JSON loading, validation, fallback, and saving.
- `src/GameOverlay.Core/OverlayGeometry.cs`: computes line and center-point rectangles.
- `src/GameOverlay.App/GameOverlay.App.csproj`: Windows executable with WPF and WinForms enabled.
- `src/GameOverlay.App/Program.cs`: application entry point.
- `src/GameOverlay.App/GameOverlayApplication.cs`: tray lifecycle, config wiring, and overlay toggle state.
- `src/GameOverlay.App/OverlayWindow.cs`: WPF transparent topmost click-through full-screen overlay window.
- `src/GameOverlay.App/HotkeyWindow.cs`: hidden `HwndSource` message window for global hotkey registration.
- `src/GameOverlay.App/Win32.cs`: P/Invoke constants and methods.
- `tests/GameOverlay.Tests/GameOverlay.Tests.csproj`: dependency-free console test runner.
- `tests/GameOverlay.Tests/Program.cs`: assertion helpers and tests for config and geometry.
- `README.md`: run, build, hotkey, config, and full-screen compatibility notes.

## Task 1: Scaffold Solution

**Files:**
- Create: `GameOverlay.sln`
- Create: `src/GameOverlay.Core/GameOverlay.Core.csproj`
- Create: `src/GameOverlay.App/GameOverlay.App.csproj`
- Create: `tests/GameOverlay.Tests/GameOverlay.Tests.csproj`
- Create: `tests/GameOverlay.Tests/Program.cs`

- [ ] **Step 1: Create the solution and projects**

Run:

```powershell
dotnet new sln -n GameOverlay
dotnet new classlib -n GameOverlay.Core -o src/GameOverlay.Core
dotnet new console -n GameOverlay.App -o src/GameOverlay.App
dotnet new console -n GameOverlay.Tests -o tests/GameOverlay.Tests
dotnet sln GameOverlay.sln add src/GameOverlay.Core/GameOverlay.Core.csproj
dotnet sln GameOverlay.sln add src/GameOverlay.App/GameOverlay.App.csproj
dotnet sln GameOverlay.sln add tests/GameOverlay.Tests/GameOverlay.Tests.csproj
dotnet add src/GameOverlay.App/GameOverlay.App.csproj reference src/GameOverlay.Core/GameOverlay.Core.csproj
dotnet add tests/GameOverlay.Tests/GameOverlay.Tests.csproj reference src/GameOverlay.Core/GameOverlay.Core.csproj
```

Expected: solution contains all three projects.

- [ ] **Step 2: Normalize project files**

Set `src/GameOverlay.Core/GameOverlay.Core.csproj` to:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.0</TargetFramework>
    <LangVersion>8.0</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Set `src/GameOverlay.App/GameOverlay.App.csproj` to:

```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>netcoreapp3.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <LangVersion>8.0</LangVersion>
    <Nullable>enable</Nullable>
    <AssemblyName>GameOverlay</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\GameOverlay.Core\GameOverlay.Core.csproj" />
  </ItemGroup>
</Project>
```

Set `tests/GameOverlay.Tests/GameOverlay.Tests.csproj` to:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.0</TargetFramework>
    <LangVersion>8.0</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\GameOverlay.Core\GameOverlay.Core.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Add a failing placeholder test runner**

Replace `tests/GameOverlay.Tests/Program.cs` with:

```csharp
using System;

namespace GameOverlay.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            Console.WriteLine("GameOverlay tests are not implemented yet.");
            return 1;
        }
    }
}
```

- [ ] **Step 4: Run build and test runner**

Run:

```powershell
dotnet build GameOverlay.sln
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: build passes; test runner exits with code `1`.

- [ ] **Step 5: Commit scaffold**

```powershell
git add GameOverlay.sln src tests
git commit -m "chore: scaffold game overlay solution"
```

## Task 2: Add Configuration Defaults and Validation

**Files:**
- Create: `src/GameOverlay.Core/OverlayConfig.cs`
- Create: `src/GameOverlay.Core/ConfigStore.cs`
- Modify: `tests/GameOverlay.Tests/Program.cs`

- [ ] **Step 1: Write failing config tests**

Replace `tests/GameOverlay.Tests/Program.cs` with:

```csharp
using System;
using System.Drawing;
using System.IO;
using GameOverlay.Core;

namespace GameOverlay.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                DefaultConfigMatchesReference();
                InvalidConfigFallsBackToDefaults();
                MissingConfigIsCreated();
                Console.WriteLine("All tests passed.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void DefaultConfigMatchesReference()
        {
            OverlayConfig config = OverlayConfig.CreateDefault();
            AssertEqual("#ffff00", config.LineColor, "line color");
            AssertEqual(0.35, config.LineOpacity, "line opacity");
            AssertEqual(28, config.LineThickness, "line thickness");
            AssertEqual(360, config.HorizontalLineLength, "horizontal length");
            AssertEqual(320, config.VerticalLineLength, "vertical length");
            AssertEqual(52, config.CenterGap, "center gap");
            AssertEqual(26, config.CenterPointWidth, "center point width");
            AssertEqual(40, config.CenterPointHeight, "center point height");
            AssertEqual(0.9, config.CenterPointOpacity, "center point opacity");
            AssertEqual("Ctrl+Alt+X", config.ToggleHotkey, "hotkey");
        }

        private static void InvalidConfigFallsBackToDefaults()
        {
            OverlayConfig invalid = new OverlayConfig
            {
                LineColor = "yellow",
                LineOpacity = 2,
                LineThickness = -1,
                HorizontalLineLength = 0,
                VerticalLineLength = 0,
                CenterGap = -10,
                CenterPointWidth = 0,
                CenterPointHeight = 0,
                CenterPointOpacity = -1,
                ToggleHotkey = ""
            };

            OverlayConfig normalized = invalid.Normalize();
            AssertEqual("#ffff00", normalized.LineColor, "normalized line color");
            AssertEqual(28, normalized.LineThickness, "normalized line thickness");
            AssertEqual("Ctrl+Alt+X", normalized.ToggleHotkey, "normalized hotkey");
        }

        private static void MissingConfigIsCreated()
        {
            string dir = Path.Combine(Path.GetTempPath(), "game-overlay-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "config.json");
            OverlayConfig config = ConfigStore.LoadOrCreate(path);
            AssertEqual(true, File.Exists(path), "config file exists");
            AssertEqual(28, config.LineThickness, "created config thickness");
            Directory.Delete(dir, true);
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!object.Equals(expected, actual))
            {
                throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
            }
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: FAIL because `OverlayConfig` and `ConfigStore` do not exist.

- [ ] **Step 3: Implement config**

Create `src/GameOverlay.Core/OverlayConfig.cs`:

```csharp
using System.Text.RegularExpressions;

namespace GameOverlay.Core
{
    public sealed class OverlayConfig
    {
        private static readonly Regex HexColor = new Regex("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        public bool OverlayEnabled { get; set; } = true;
        public string ToggleHotkey { get; set; } = "Ctrl+Alt+X";
        public string LineColor { get; set; } = "#ffff00";
        public double LineOpacity { get; set; } = 0.35;
        public int LineThickness { get; set; } = 28;
        public int HorizontalLineLength { get; set; } = 360;
        public int VerticalLineLength { get; set; } = 320;
        public int CenterGap { get; set; } = 52;
        public int CenterPointWidth { get; set; } = 26;
        public int CenterPointHeight { get; set; } = 40;
        public double CenterPointOpacity { get; set; } = 0.9;

        public static OverlayConfig CreateDefault()
        {
            return new OverlayConfig();
        }

        public OverlayConfig Normalize()
        {
            OverlayConfig defaults = CreateDefault();
            return new OverlayConfig
            {
                OverlayEnabled = OverlayEnabled,
                ToggleHotkey = string.IsNullOrWhiteSpace(ToggleHotkey) ? defaults.ToggleHotkey : ToggleHotkey,
                LineColor = HexColor.IsMatch(LineColor ?? string.Empty) ? LineColor : defaults.LineColor,
                LineOpacity = ClampOpacity(LineOpacity, defaults.LineOpacity),
                LineThickness = Positive(LineThickness, defaults.LineThickness),
                HorizontalLineLength = Positive(HorizontalLineLength, defaults.HorizontalLineLength),
                VerticalLineLength = Positive(VerticalLineLength, defaults.VerticalLineLength),
                CenterGap = NonNegative(CenterGap, defaults.CenterGap),
                CenterPointWidth = Positive(CenterPointWidth, defaults.CenterPointWidth),
                CenterPointHeight = Positive(CenterPointHeight, defaults.CenterPointHeight),
                CenterPointOpacity = ClampOpacity(CenterPointOpacity, defaults.CenterPointOpacity)
            };
        }

        private static double ClampOpacity(double value, double fallback)
        {
            return value >= 0 && value <= 1 ? value : fallback;
        }

        private static int Positive(int value, int fallback)
        {
            return value > 0 ? value : fallback;
        }

        private static int NonNegative(int value, int fallback)
        {
            return value >= 0 ? value : fallback;
        }
    }
}
```

Create `src/GameOverlay.Core/ConfigStore.cs`:

```csharp
using System;
using System.IO;
using System.Text.Json;

namespace GameOverlay.Core
{
    public static class ConfigStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static OverlayConfig LoadOrCreate(string path)
        {
            OverlayConfig config = Read(path).Normalize();
            Save(path, config);
            return config;
        }

        public static void Save(string path, OverlayConfig config)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(config.Normalize(), Options);
            File.WriteAllText(path, json);
        }

        private static OverlayConfig Read(string path)
        {
            if (!File.Exists(path))
            {
                return OverlayConfig.CreateDefault();
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<OverlayConfig>(json, Options) ?? OverlayConfig.CreateDefault();
            }
            catch (JsonException)
            {
                return OverlayConfig.CreateDefault();
            }
            catch (IOException)
            {
                return OverlayConfig.CreateDefault();
            }
        }
    }
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: PASS with `All tests passed.`

- [ ] **Step 5: Commit config**

```powershell
git add src/GameOverlay.Core tests/GameOverlay.Tests
git commit -m "feat: add overlay configuration"
```

## Task 3: Add Overlay Geometry

**Files:**
- Create: `src/GameOverlay.Core/OverlayGeometry.cs`
- Modify: `tests/GameOverlay.Tests/Program.cs`

- [ ] **Step 1: Add failing geometry test**

Add this method to `tests/GameOverlay.Tests/Program.cs` and call it from `Main` after `MissingConfigIsCreated();`:

```csharp
private static void GeometryPlacesSegmentsAroundCenter()
{
    OverlayConfig config = OverlayConfig.CreateDefault();
    OverlayLayout layout = OverlayGeometry.Calculate(new Size(1920, 1080), config);

    AssertEqual(new Rectangle(535, 526, 360, 28), layout.LeftLine, "left line");
    AssertEqual(new Rectangle(1025, 526, 360, 28), layout.RightLine, "right line");
    AssertEqual(new Rectangle(946, 148, 28, 320), layout.TopLine, "top line");
    AssertEqual(new Rectangle(946, 612, 28, 320), layout.BottomLine, "bottom line");
    AssertEqual(new Rectangle(947, 520, 26, 40), layout.CenterPoint, "center point");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: FAIL because `OverlayGeometry` and `OverlayLayout` do not exist.

- [ ] **Step 3: Implement geometry**

Create `src/GameOverlay.Core/OverlayGeometry.cs`:

```csharp
using System.Drawing;

namespace GameOverlay.Core
{
    public sealed class OverlayLayout
    {
        public OverlayLayout(Rectangle leftLine, Rectangle rightLine, Rectangle topLine, Rectangle bottomLine, Rectangle centerPoint)
        {
            LeftLine = leftLine;
            RightLine = rightLine;
            TopLine = topLine;
            BottomLine = bottomLine;
            CenterPoint = centerPoint;
        }

        public Rectangle LeftLine { get; }
        public Rectangle RightLine { get; }
        public Rectangle TopLine { get; }
        public Rectangle BottomLine { get; }
        public Rectangle CenterPoint { get; }
    }

    public static class OverlayGeometry
    {
        public static OverlayLayout Calculate(Size canvas, OverlayConfig config)
        {
            OverlayConfig normalized = config.Normalize();
            int centerX = canvas.Width / 2;
            int centerY = canvas.Height / 2;
            int halfThickness = normalized.LineThickness / 2;

            Rectangle centerPoint = new Rectangle(
                centerX - normalized.CenterPointWidth / 2,
                centerY - normalized.CenterPointHeight / 2,
                normalized.CenterPointWidth,
                normalized.CenterPointHeight);

            int leftEnd = centerX - normalized.CenterGap - normalized.CenterPointWidth / 2;
            int rightStart = centerX + normalized.CenterGap + normalized.CenterPointWidth / 2;
            int topEnd = centerY - normalized.CenterGap - normalized.CenterPointHeight / 2;
            int bottomStart = centerY + normalized.CenterGap + normalized.CenterPointHeight / 2;

            Rectangle leftLine = new Rectangle(
                leftEnd - normalized.HorizontalLineLength,
                centerY - halfThickness,
                normalized.HorizontalLineLength,
                normalized.LineThickness);

            Rectangle rightLine = new Rectangle(
                rightStart,
                centerY - halfThickness,
                normalized.HorizontalLineLength,
                normalized.LineThickness);

            Rectangle topLine = new Rectangle(
                centerX - halfThickness,
                topEnd - normalized.VerticalLineLength,
                normalized.LineThickness,
                normalized.VerticalLineLength);

            Rectangle bottomLine = new Rectangle(
                centerX - halfThickness,
                bottomStart,
                normalized.LineThickness,
                normalized.VerticalLineLength);

            return new OverlayLayout(leftLine, rightLine, topLine, bottomLine, centerPoint);
        }
    }
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: PASS.

- [ ] **Step 5: Commit geometry**

```powershell
git add src/GameOverlay.Core tests/GameOverlay.Tests
git commit -m "feat: add overlay geometry"
```

## Task 4: Add WPF Overlay Window and Renderer

**Files:**
- Create: `src/GameOverlay.App/Win32.cs`
- Create: `src/GameOverlay.App/OverlayWindow.cs`
- Modify: `src/GameOverlay.App/Program.cs`

- [ ] **Step 1: Create Win32 helpers**

Create `src/GameOverlay.App/Win32.cs`:

```csharp
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GameOverlay.App
{
    internal static class Win32
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void MakeWindowClickThrough(System.Windows.Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            int style = GetWindowLong(handle, GWL_EXSTYLE);
            SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        }
    }
}
```

- [ ] **Step 2: Create overlay window**

Create `src/GameOverlay.App/OverlayWindow.cs`:

```csharp
using GameOverlay.Core;
using Forms = System.Windows.Forms;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GameOverlay.App
{
    internal sealed class OverlayWindow : Window
    {
        private OverlayConfig _config;

        public OverlayWindow(OverlayConfig config)
        {
            _config = config.Normalize();
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            TopMost = true;
            ShowActivated = false;
            RefreshScreenBounds();
            SourceInitialized += (_, __) => Win32.MakeWindowClickThrough(this);
        }

        public void UpdateConfig(OverlayConfig config)
        {
            _config = config.Normalize();
            InvalidateVisual();
        }

        public void RefreshScreenBounds()
        {
            Forms.Screen screen = Forms.Screen.PrimaryScreen;
            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var size = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
            OverlayLayout layout = OverlayGeometry.Calculate(size, _config);
            Brush lineBrush = new SolidColorBrush(WithOpacity(_config.LineColor, _config.LineOpacity));
            Brush pointBrush = new SolidColorBrush(WithOpacity(_config.LineColor, _config.CenterPointOpacity));
            drawingContext.DrawRectangle(lineBrush, null, ToRect(layout.LeftLine));
            drawingContext.DrawRectangle(lineBrush, null, ToRect(layout.RightLine));
            drawingContext.DrawRectangle(lineBrush, null, ToRect(layout.TopLine));
            drawingContext.DrawRectangle(lineBrush, null, ToRect(layout.BottomLine));
            drawingContext.DrawRoundedRectangle(pointBrush, null, ToRect(layout.CenterPoint), 5, 5);
        }

        private static Color WithOpacity(string hex, double opacity)
        {
            byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            return Color.FromArgb((byte)(opacity * 255), r, g, b);
        }

        private static Rect ToRect(System.Drawing.Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }
}
```

- [ ] **Step 3: Temporarily show overlay from app entry**

Replace `src/GameOverlay.App/Program.cs` with:

```csharp
using GameOverlay.Core;
using System;
using System.Windows;

namespace GameOverlay.App
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var app = new Application();
            app.Run(new OverlayWindow(OverlayConfig.CreateDefault()));
        }
    }
}
```

- [ ] **Step 4: Build app**

Run:

```powershell
dotnet build src/GameOverlay.App/GameOverlay.App.csproj
```

Expected: PASS.

- [ ] **Step 5: Commit overlay window**

```powershell
git add src/GameOverlay.App
git commit -m "feat: add transparent overlay window"
```

## Task 5: Add Tray, Hotkey, and App Lifecycle

**Files:**
- Create: `src/GameOverlay.App/GameOverlayApplication.cs`
- Create: `src/GameOverlay.App/HotkeyWindow.cs`
- Modify: `src/GameOverlay.App/Program.cs`

- [ ] **Step 1: Add hotkey message window**

Create `src/GameOverlay.App/HotkeyWindow.cs`:

```csharp
using System;
using System.Windows.Forms;
using System.Windows.Interop;

namespace GameOverlay.App
{
    internal sealed class HotkeyWindow : IDisposable
    {
        private const int ModAlt = 0x0001;
        private const int ModControl = 0x0002;
        private const int HotkeyId = 9001;
        private readonly HwndSource _source;
        private bool _registered;

        public HotkeyWindow()
        {
            var parameters = new HwndSourceParameters("GameOverlayHotkeyWindow")
            {
                Width = 0,
                Height = 0,
                WindowStyle = 0
            };
            _source = new HwndSource(parameters);
            _source.AddHook(WndProc);
        }

        public event EventHandler? HotkeyPressed;

        public bool RegisterToggleHotkey()
        {
            _registered = Win32.RegisterHotKey(_source.Handle, HotkeyId, ModControl | ModAlt, (int)Keys.X);
            return _registered;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_registered)
            {
                Win32.UnregisterHotKey(_source.Handle, HotkeyId);
            }

            _source.RemoveHook(WndProc);
            _source.Dispose();
        }
    }
}
```

- [ ] **Step 2: Add application lifecycle**

Create `src/GameOverlay.App/GameOverlayApplication.cs`:

```csharp
using System;
using System.IO;
using System.Windows;
using GameOverlay.Core;
using Forms = System.Windows.Forms;

namespace GameOverlay.App
{
    internal sealed class GameOverlayApplication : Application
    {
        private readonly OverlayWindow _overlay;
        private readonly Forms.NotifyIcon _trayIcon;
        private readonly HotkeyWindow _hotkeyWindow;
        private readonly string _configPath;
        private OverlayConfig _config;

        public GameOverlayApplication()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameOverlay",
                "config.json");
            _config = ConfigStore.LoadOrCreate(_configPath);
            _overlay = new OverlayWindow(_config);
            _hotkeyWindow = new HotkeyWindow();
            _hotkeyWindow.HotkeyPressed += (_, __) => ToggleOverlay();
            _hotkeyWindow.RegisterToggleHotkey();

            _trayIcon = new Forms.NotifyIcon
            {
                Text = "Game Overlay",
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                ContextMenuStrip = BuildMenu()
            };

            if (_config.OverlayEnabled)
            {
                _overlay.Show();
            }
        }

        private Forms.ContextMenuStrip BuildMenu()
        {
            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add("显示/隐藏 Overlay", null, (_, __) => ToggleOverlay());
            menu.Items.Add("退出", null, (_, __) => Exit());
            return menu;
        }

        private void ToggleOverlay()
        {
            _config.OverlayEnabled = !_overlay.Visible;
            if (_config.OverlayEnabled)
            {
                _overlay.RefreshScreenBounds();
                _overlay.Show();
            }
            else
            {
                _overlay.Hide();
            }

            ConfigStore.Save(_configPath, _config);
        }

        private void Exit()
        {
            _trayIcon.Visible = false;
            _hotkeyWindow.Dispose();
            _overlay.Close();
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
```

- [ ] **Step 3: Run app context from entry**

Replace `src/GameOverlay.App/Program.cs` with:

```csharp
using System;

namespace GameOverlay.App
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var app = new GameOverlayApplication();
            app.Run();
        }
    }
}
```

- [ ] **Step 4: Build app**

Run:

```powershell
dotnet build GameOverlay.sln
```

Expected: PASS.

- [ ] **Step 5: Commit tray and hotkey**

```powershell
git add src/GameOverlay.App
git commit -m "feat: add tray and hotkey controls"
```

## Task 6: Add Documentation and Final Verification

**Files:**
- Create: `README.md`

- [ ] **Step 1: Create README**

Create `README.md`:

```markdown
# Game Overlay

A small Windows tray utility that shows a centered translucent yellow reference overlay.

## Run

```powershell
dotnet run --project src/GameOverlay.App/GameOverlay.App.csproj
```

## Controls

- Tray menu: show/hide overlay, exit
- Hotkey: `Ctrl+Alt+X`

## Config

The app writes its config to:

```text
%APPDATA%\GameOverlay\config.json
```

Important default values:

- Line color: `#ffff00`
- Line opacity: `0.35`
- Line thickness: `28`
- Stretch lines to screen edges: `true`
- Horizontal line length when edge stretch is disabled: `360`
- Vertical line length when edge stretch is disabled: `320`
- Center gap: `52`
- Center point size: `26x40`
- Center point opacity: `0.9`

## Full-screen compatibility

This first version uses a safe transparent topmost window. It should work over desktop windows, windowed games, and many borderless full-screen games.

True exclusive full-screen games may appear above the overlay. This version does not inject into games and does not hook DirectX, which avoids common anti-cheat risks.
```

- [ ] **Step 2: Run automated tests**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: PASS.

- [ ] **Step 3: Build release**

Run:

```powershell
dotnet publish src/GameOverlay.App/GameOverlay.App.csproj -c Release -r win-x64 --self-contained false
```

Expected: release files are created under `src/GameOverlay.App/bin/Release/netcoreapp3.0/win-x64/publish`.

- [ ] **Step 4: Manual smoke test**

Run:

```powershell
dotnet run --project src/GameOverlay.App/GameOverlay.App.csproj
```

Expected:

- Tray icon appears.
- Overlay appears at screen center.
- Yellow line segments are translucent.
- Center point is visible.
- Mouse clicks pass through the overlay.
- `Ctrl+Alt+X` hides and shows the overlay.
- Tray exit closes the app.

- [ ] **Step 5: Commit docs and verification**

```powershell
git add README.md
git commit -m "docs: add game overlay usage"
```
