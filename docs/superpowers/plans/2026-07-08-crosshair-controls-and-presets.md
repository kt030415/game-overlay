# Crosshair Controls and Presets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add adjustable outer-line and center-crosshair controls plus built-in firearm-style crosshair presets.

**Architecture:** Keep persisted shape state in `OverlayConfig`. Add a testable Core preset catalog and apply helper, then extend the WPF settings window to edit existing config fields and apply presets through the existing `ConfigChanged` event.

**Tech Stack:** C# 8, .NET Core 3.0, WPF, existing console-based `GameOverlay.Tests` runner.

---

## File Structure

- Create `src/GameOverlay.Core/CrosshairPreset.cs`: immutable preset data and apply logic.
- Modify `tests/GameOverlay.Tests/Program.cs`: add Core tests for preset names, shape application, and preservation of unrelated settings.
- Modify `src/GameOverlay.App/SettingsWindow.cs`: add preset buttons, stretch toggle, and sliders for line and center-crosshair dimensions.
- Modify `README.md`: document new settings and built-in presets after code is complete.

### Task 1: Add Core Crosshair Presets

**Files:**
- Create: `src/GameOverlay.Core/CrosshairPreset.cs`
- Modify: `tests/GameOverlay.Tests/Program.cs`

- [ ] **Step 1: Write the failing preset catalog test**

Add a call in `Main()` after `ColorPresetsProvideRequestedColors();`:

```csharp
CrosshairPresetsProvideFirearmStyles();
```

Add this method near the other test methods:

```csharp
private static void CrosshairPresetsProvideFirearmStyles()
{
    string[] names = CrosshairPresets.Names;
    AssertEqual(5, names.Length, "crosshair preset count");
    AssertEqual("Pistol", names[0], "pistol preset name");
    AssertEqual("SMG", names[1], "smg preset name");
    AssertEqual("Rifle", names[2], "rifle preset name");
    AssertEqual("Sniper", names[3], "sniper preset name");
    AssertEqual("Shotgun", names[4], "shotgun preset name");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: build fails because `CrosshairPresets` does not exist.

- [ ] **Step 3: Add minimal preset catalog implementation**

Create `src/GameOverlay.Core/CrosshairPreset.cs`:

```csharp
namespace GameOverlay.Core
{
    public sealed class CrosshairPreset
    {
        public CrosshairPreset(
            string name,
            int lineThickness,
            int horizontalLineLength,
            int verticalLineLength,
            int centerGap,
            int centerReticleLength,
            int centerReticleThickness)
        {
            Name = name;
            LineThickness = lineThickness;
            HorizontalLineLength = horizontalLineLength;
            VerticalLineLength = verticalLineLength;
            CenterGap = centerGap;
            CenterReticleLength = centerReticleLength;
            CenterReticleThickness = centerReticleThickness;
        }

        public string Name { get; }
        public int LineThickness { get; }
        public int HorizontalLineLength { get; }
        public int VerticalLineLength { get; }
        public int CenterGap { get; }
        public int CenterReticleLength { get; }
        public int CenterReticleThickness { get; }
    }

    public static class CrosshairPresets
    {
        public static readonly CrosshairPreset Pistol = new CrosshairPreset("Pistol", 16, 220, 200, 38, 24, 4);
        public static readonly CrosshairPreset Smg = new CrosshairPreset("SMG", 22, 190, 170, 28, 26, 5);
        public static readonly CrosshairPreset Rifle = new CrosshairPreset("Rifle", 28, 360, 320, 52, 34, 6);
        public static readonly CrosshairPreset Sniper = new CrosshairPreset("Sniper", 10, 520, 460, 34, 22, 3);
        public static readonly CrosshairPreset Shotgun = new CrosshairPreset("Shotgun", 34, 180, 160, 76, 42, 8);

        public static CrosshairPreset[] BuiltIn { get; } =
        {
            Pistol,
            Smg,
            Rifle,
            Sniper,
            Shotgun
        };

        public static string[] Names
        {
            get
            {
                string[] names = new string[BuiltIn.Length];
                for (int i = 0; i < BuiltIn.Length; i++)
                {
                    names[i] = BuiltIn[i].Name;
                }

                return names;
            }
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: `All tests passed.`

### Task 2: Add Preset Application Behavior

**Files:**
- Modify: `src/GameOverlay.Core/CrosshairPreset.cs`
- Modify: `tests/GameOverlay.Tests/Program.cs`

- [ ] **Step 1: Write failing preset application tests**

Add calls in `Main()` after `CrosshairPresetsProvideFirearmStyles();`:

```csharp
CrosshairPresetApplyUpdatesShape();
CrosshairPresetApplyPreservesUserPreferences();
```

Add these methods:

```csharp
private static void CrosshairPresetApplyUpdatesShape()
{
    OverlayConfig original = OverlayConfig.CreateDefault();
    OverlayConfig applied = CrosshairPresets.Apply(original, CrosshairPresets.Shotgun);

    AssertEqual(34, applied.LineThickness, "preset line thickness");
    AssertEqual(180, applied.HorizontalLineLength, "preset horizontal length");
    AssertEqual(160, applied.VerticalLineLength, "preset vertical length");
    AssertEqual(76, applied.CenterGap, "preset center gap");
    AssertEqual(42, applied.CenterReticleLength, "preset center reticle length");
    AssertEqual(8, applied.CenterReticleThickness, "preset center reticle thickness");
}

private static void CrosshairPresetApplyPreservesUserPreferences()
{
    OverlayConfig original = OverlayConfig.CreateDefault();
    original.OverlayEnabled = false;
    original.ToggleHotkey = "Ctrl+Alt+Z";
    original.LineColor = "#0066ff";
    original.LineOpacity = 0.6;
    original.CenterReticleOpacity = 0.95;
    original.StretchLinesToEdges = false;

    OverlayConfig applied = CrosshairPresets.Apply(original, CrosshairPresets.Pistol);

    AssertEqual(false, applied.OverlayEnabled, "preset preserves enabled state");
    AssertEqual("Ctrl+Alt+Z", applied.ToggleHotkey, "preset preserves hotkey");
    AssertEqual("#0066ff", applied.LineColor, "preset preserves color");
    AssertEqual(0.6, applied.LineOpacity, "preset preserves line opacity");
    AssertEqual(0.95, applied.CenterReticleOpacity, "preset preserves reticle opacity");
    AssertEqual(false, applied.StretchLinesToEdges, "preset preserves stretch setting");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: build fails because `CrosshairPresets.Apply` does not exist.

- [ ] **Step 3: Implement apply helper**

Add this method to `CrosshairPresets`:

```csharp
public static OverlayConfig Apply(OverlayConfig config, CrosshairPreset preset)
{
    OverlayConfig normalized = config.Normalize();
    return new OverlayConfig
    {
        OverlayEnabled = normalized.OverlayEnabled,
        ToggleHotkey = normalized.ToggleHotkey,
        LineColor = normalized.LineColor,
        LineOpacity = normalized.LineOpacity,
        LineThickness = preset.LineThickness,
        HorizontalLineLength = preset.HorizontalLineLength,
        VerticalLineLength = preset.VerticalLineLength,
        CenterGap = preset.CenterGap,
        CenterReticleLength = preset.CenterReticleLength,
        CenterReticleThickness = preset.CenterReticleThickness,
        CenterReticleOpacity = normalized.CenterReticleOpacity,
        StretchLinesToEdges = normalized.StretchLinesToEdges
    }.Normalize();
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
```

Expected: `All tests passed.`

### Task 3: Extend Settings Window Controls

**Files:**
- Modify: `src/GameOverlay.App/SettingsWindow.cs`

- [ ] **Step 1: Update settings state fields**

Add fields:

```csharp
private TextBlock? _opacityValue;
private CheckBox? _stretchCheckBox;
private readonly System.Collections.Generic.Dictionary<string, Slider> _sliders =
    new System.Collections.Generic.Dictionary<string, Slider>();
private readonly System.Collections.Generic.Dictionary<string, TextBlock> _sliderValues =
    new System.Collections.Generic.Dictionary<string, TextBlock>();
private OverlayConfig _config;
```

- [ ] **Step 2: Increase window size and layout**

In the constructor, set:

```csharp
Width = 420;
Height = 560;
```

Build root with a scroll viewer:

```csharp
var root = new StackPanel
{
    Margin = new Thickness(14)
};

root.Children.Add(BuildColorBar());
root.Children.Add(BuildOpacityControl());
root.Children.Add(BuildPresetBar());
root.Children.Add(BuildStretchControl());
root.Children.Add(BuildSlider("LineThickness", "线宽", 4, 48, _config.LineThickness, value => _config.LineThickness = value));
root.Children.Add(BuildSlider("HorizontalLineLength", "横线长度", 60, 800, _config.HorizontalLineLength, value => _config.HorizontalLineLength = value));
root.Children.Add(BuildSlider("VerticalLineLength", "竖线长度", 60, 800, _config.VerticalLineLength, value => _config.VerticalLineLength = value));
root.Children.Add(BuildSlider("CenterGap", "中心间隙", 0, 140, _config.CenterGap, value => _config.CenterGap = value));
root.Children.Add(BuildSlider("CenterReticleLength", "准星长度", 4, 80, _config.CenterReticleLength, value => _config.CenterReticleLength = value));
root.Children.Add(BuildSlider("CenterReticleThickness", "准星宽度", 1, 24, _config.CenterReticleThickness, value => _config.CenterReticleThickness = value));

Content = new ScrollViewer
{
    Content = root,
    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
};
```

- [ ] **Step 3: Add preset and shape control builders**

Add these methods:

```csharp
private UIElement BuildPresetBar()
{
    var panel = new UniformGrid
    {
        Columns = CrosshairPresets.BuiltIn.Length,
        Margin = new Thickness(0, 0, 0, 12)
    };

    foreach (CrosshairPreset preset in CrosshairPresets.BuiltIn)
    {
        var button = new Button
        {
            Content = preset.Name,
            Height = 28,
            Margin = new Thickness(3),
            ToolTip = preset.Name
        };
        button.Click += (_, __) => ApplyPreset(preset);
        panel.Children.Add(button);
    }

    return panel;
}

private UIElement BuildStretchControl()
{
    _stretchCheckBox = new CheckBox
    {
        Content = "四条线延伸到屏幕边缘",
        IsChecked = _config.StretchLinesToEdges,
        Margin = new Thickness(0, 0, 0, 10)
    };
    _stretchCheckBox.Checked += (_, __) => ApplyStretch(true);
    _stretchCheckBox.Unchecked += (_, __) => ApplyStretch(false);
    return _stretchCheckBox;
}

private UIElement BuildSlider(string key, string labelText, int minimum, int maximum, int value, Action<int> apply)
{
    var panel = new DockPanel
    {
        Margin = new Thickness(0, 0, 0, 8)
    };

    var label = new TextBlock
    {
        Width = 76,
        VerticalAlignment = VerticalAlignment.Center,
        Text = labelText
    };

    var valueLabel = new TextBlock
    {
        Width = 44,
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Right,
        Text = value.ToString(CultureInfo.InvariantCulture)
    };

    var slider = new Slider
    {
        Minimum = minimum,
        Maximum = maximum,
        Value = value,
        TickFrequency = 1,
        IsSnapToTickEnabled = true,
        VerticalAlignment = VerticalAlignment.Center
    };
    slider.ValueChanged += (_, args) =>
    {
        int rounded = (int)Math.Round(args.NewValue);
        apply(rounded);
        valueLabel.Text = rounded.ToString(CultureInfo.InvariantCulture);
        PublishConfig();
    };

    _sliders[key] = slider;
    _sliderValues[key] = valueLabel;

    DockPanel.SetDock(label, Dock.Left);
    DockPanel.SetDock(valueLabel, Dock.Right);
    panel.Children.Add(label);
    panel.Children.Add(valueLabel);
    panel.Children.Add(slider);
    return panel;
}
```

- [ ] **Step 4: Add apply and refresh helpers**

Add these methods:

```csharp
private void ApplyStretch(bool stretch)
{
    _config.StretchLinesToEdges = stretch;
    PublishConfig();
}

private void ApplyPreset(CrosshairPreset preset)
{
    _config = CrosshairPresets.Apply(_config, preset);
    RefreshControls();
    PublishConfig();
}

private void RefreshControls()
{
    if (_stretchCheckBox != null)
    {
        _stretchCheckBox.IsChecked = _config.StretchLinesToEdges;
    }

    SetSlider("LineThickness", _config.LineThickness);
    SetSlider("HorizontalLineLength", _config.HorizontalLineLength);
    SetSlider("VerticalLineLength", _config.VerticalLineLength);
    SetSlider("CenterGap", _config.CenterGap);
    SetSlider("CenterReticleLength", _config.CenterReticleLength);
    SetSlider("CenterReticleThickness", _config.CenterReticleThickness);
}

private void SetSlider(string key, int value)
{
    if (_sliders.TryGetValue(key, out Slider slider))
    {
        slider.Value = value;
    }

    if (_sliderValues.TryGetValue(key, out TextBlock label))
    {
        label.Text = value.ToString(CultureInfo.InvariantCulture);
    }
}
```

- [ ] **Step 5: Build app to verify UI code compiles**

Run:

```powershell
dotnet build src/GameOverlay.App/GameOverlay.App.csproj
```

Expected: build succeeds.

### Task 4: Document and Verify

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Update README controls**

Replace the settings bullet under `## Controls` with:

```markdown
- Tray settings: color presets, opacity slider, line size sliders, center crosshair sliders, stretch-to-edges toggle, and firearm-style crosshair presets
```

Add this list after the default values:

```markdown
Built-in crosshair presets:

- Pistol
- SMG
- Rifle
- Sniper
- Shotgun
```

- [ ] **Step 2: Run full test and build verification**

Run:

```powershell
dotnet run --project tests/GameOverlay.Tests/GameOverlay.Tests.csproj
dotnet build src/GameOverlay.App/GameOverlay.App.csproj
```

Expected:

```text
All tests passed.
Build succeeded.
```

- [ ] **Step 3: Commit implementation**

Run:

```powershell
git add src/GameOverlay.Core/CrosshairPreset.cs tests/GameOverlay.Tests/Program.cs src/GameOverlay.App/SettingsWindow.cs README.md docs/superpowers/plans/2026-07-08-crosshair-controls-and-presets.md
git commit -m "Add crosshair controls and presets"
```
