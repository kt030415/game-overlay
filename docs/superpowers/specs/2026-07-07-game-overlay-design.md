# Game Overlay Design

## Goal

Build a small Windows utility that displays a centered yellow reference overlay above games and other full-screen content where the Windows desktop compositor allows it. The first version favors a safe, non-injected overlay over aggressive game hooking.

## User Experience

- The app starts without a main window.
- A system tray icon is always available while the app is running.
- The tray menu contains:
  - Show or hide overlay
  - Open settings
  - Exit
- A global hotkey toggles the overlay. The default hotkey is `Ctrl+Alt+X`.
- The overlay is click-through, so it should not block mouse input to the game.

## Overlay Appearance

The overlay is fixed at the center of the primary display.

Visual elements:

- Left horizontal translucent yellow line
- Right horizontal translucent yellow line
- Upper vertical translucent yellow line
- Lower vertical translucent yellow line
- Center yellow dot or small rounded square

The four line segments leave a clear gap around the center crosshair. The line style should resemble the provided reference image: thick, visible, and translucent. The center crosshair should be more prominent than the guide lines.

Default visual parameters:

- Line color: `#ffff00`
- Line opacity: `0.35`
- Line thickness: `28px`
- Stretch lines to the primary screen edges by default
- Horizontal line length when edge stretch is disabled: `360px` on each side
- Vertical line length when edge stretch is disabled: `320px` above and below center
- Center gap: `52px` from the center crosshair area to each line segment
- Center crosshair length: `34px`
- Center crosshair thickness: `6px`
- Center crosshair opacity: `0.9`

## Configuration

The app stores settings in a local JSON file. The first implementation may ship with defaults and create the file on first run.

Configurable values:

- Overlay enabled state
- Hotkey
- Line color
- Color preset
- Line opacity
- Line thickness
- Horizontal line length
- Vertical line length
- Stretch lines to screen edges
- Center gap
- Center crosshair length
- Center crosshair thickness
- Center crosshair opacity

## Architecture

The app is split into focused components:

- `App`: process startup, shutdown, and shared wiring
- `TrayController`: tray icon, tray menu, and menu events
- `HotkeyController`: global hotkey registration and toggle event
- `ConfigStore`: read, validate, default, and write JSON settings
- `OverlayWindow`: transparent topmost click-through window
- `OverlayRenderer`: draws the centered yellow line segments and center crosshair

The first rendering backend uses a normal transparent layered window. The code should keep rendering separate from tray, hotkey, and config logic so a future DirectX hook backend can be added without replacing the rest of the app.

## Full-Screen Compatibility

The first version targets:

- Desktop windows
- Windowed games
- Borderless full-screen games
- Some full-screen modes that still allow topmost layered windows

Known limitation:

- True exclusive full-screen games may appear above the overlay.
- Games with anti-cheat may block overlays or treat injected overlays as suspicious.
- The first version does not inject into game processes and does not hook DirectX.

## Error Handling

- If the hotkey cannot be registered, the app still runs and records the error.
- If the config file is missing or invalid, defaults are used and a valid config is rewritten.
- If the tray icon cannot be created, the app exits with a clear error.
- If overlay window creation fails, the app exits with a clear error.

## Testing

Manual checks:

- App starts and shows a tray icon.
- Tray menu toggles overlay visibility.
- `Ctrl+Alt+X` toggles overlay visibility.
- Overlay is centered on the primary display.
- Overlay remains click-through.
- Lines are translucent yellow and do not pass through the center crosshair.
- Center crosshair is visible.
- Overlay updates correctly after display resolution changes.
- Overlay appears above normal windows.
- Overlay appears above a borderless full-screen game or test window.

Automated checks where practical:

- Config defaults are generated correctly.
- Invalid config falls back to defaults.
- Overlay geometry calculations place all segments around the center gap.
- Hotkey toggle state updates overlay visibility state.
