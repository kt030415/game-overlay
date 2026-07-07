# Game Overlay

A small Windows tray utility that shows a centered translucent yellow reference overlay.

## Run

```powershell
dotnet run --project src/GameOverlay.App/GameOverlay.App.csproj
```

## Build Release

```powershell
dotnet publish src/GameOverlay.App/GameOverlay.App.csproj -c Release -r win-x64 --self-contained false
```

The published app is written to:

```text
src\GameOverlay.App\bin\Release\netcoreapp3.0\win-x64\publish
```

Run `GameOverlay.exe` from that folder.

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

## Full-screen Compatibility

This first version uses a safe transparent topmost window. It should work over desktop windows, windowed games, and many borderless full-screen games.

True exclusive full-screen games may appear above the overlay. This version does not inject into games and does not hook DirectX, which avoids common anti-cheat risks.
