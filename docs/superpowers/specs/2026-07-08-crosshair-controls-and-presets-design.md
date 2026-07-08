# Crosshair Controls and Presets Design

## Goal

Add settings for adjusting the four overlay guide lines and the center crosshair, plus built-in firearm-style crosshair presets. The feature should reuse the existing overlay configuration fields and preserve compatibility with existing config files.

## User Experience

The settings window expands from the current color and opacity controls into a compact crosshair editor.

Controls:

- Color presets remain available.
- Line opacity remains available.
- Line thickness controls the width of all four outer guide lines.
- Horizontal line length controls the left and right guide lines when edge stretch is disabled.
- Vertical line length controls the top and bottom guide lines when edge stretch is disabled.
- Stretch-to-edges toggle keeps the current full-screen guide behavior. When enabled, length values are still saved but the visible lines extend to the screen edges.
- Center gap controls the distance between the center reticle area and the four guide lines.
- Center reticle length controls the length of the small central crosshair strokes.
- Center reticle thickness controls the width of the central crosshair strokes.
- Built-in preset buttons apply coordinated values for firearm-style crosshairs.

Built-in preset names:

- Pistol
- SMG
- Rifle
- Sniper
- Shotgun

Preset intent:

- Pistol: medium gap, short lines, thinner stroke.
- SMG: tight gap, short lines, slightly thicker stroke.
- Rifle: balanced general-purpose shape close to the current default.
- Sniper: longer, thinner guide lines with a small center reticle.
- Shotgun: wider gap, short and thick guide lines.

## Architecture

Reuse `OverlayConfig` as the persisted shape model. No config migration is needed because all adjustable values already exist:

- `LineThickness`
- `HorizontalLineLength`
- `VerticalLineLength`
- `CenterGap`
- `CenterReticleLength`
- `CenterReticleThickness`
- `StretchLinesToEdges`
- Existing color and opacity fields

Add a Core-layer preset model so preset definitions are testable and not hard-coded only in WPF UI code.

New Core concept:

- `CrosshairPreset`: display name plus the shape values that should be applied.
- `CrosshairPresets`: exposes built-in presets and an apply helper that returns a normalized `OverlayConfig`.

The settings window remains responsible for UI controls and publishing updated config through the existing `ConfigChanged` event.

## Components

`GameOverlay.Core`

- Add crosshair preset definitions.
- Add tests covering preset availability and application behavior.
- Keep geometry calculation unchanged unless tests expose a bug.

`GameOverlay.App`

- Increase settings window size to fit the additional controls.
- Add sliders for line and center-reticle dimensions.
- Add a checkbox for stretch-to-edges.
- Add preset buttons that apply Core presets and refresh control values.
- Continue calling `ConfigChanged` after each adjustment so the overlay updates live.

## Error Handling

All values applied by sliders or presets flow through `OverlayConfig.Normalize()`. Invalid config files continue to fall back to defaults for invalid numeric values.

Preset application should not mutate the source config object in place. It should return a normalized config so UI code can safely assign and publish the result.

## Testing

Automated tests:

- Built-in presets include Pistol, SMG, Rifle, Sniper, and Shotgun.
- Applying a preset updates line length, line thickness, center gap, center reticle length, and center reticle thickness.
- Applying a preset preserves unrelated user preferences such as color, opacity, overlay enabled state, hotkey, and stretch-to-edges.
- Existing config normalization and geometry tests continue to pass.

Manual checks:

- Opening settings shows color, opacity, shape sliders, stretch toggle, and preset buttons.
- Moving each slider updates the overlay immediately.
- Toggling stretch-to-edges updates the visible line behavior immediately.
- Each preset produces a visibly different crosshair shape.
- Closing and reopening the app preserves the last adjusted values.
