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
        private SettingsWindow? _settingsWindow;

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
            bool hotkeyRegistered = _hotkeyWindow.RegisterToggleHotkey();

            _trayIcon = new Forms.NotifyIcon
            {
                Text = hotkeyRegistered ? "Game Overlay" : "Game Overlay - hotkey unavailable",
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
            menu.Items.Add("设置...", null, (_, __) => ShowSettings());
            menu.Items.Add("退出", null, (_, __) => ExitApplication());
            return menu;
        }

        private void ToggleOverlay()
        {
            _config.OverlayEnabled = !_overlay.IsVisible;
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

        private void ShowSettings()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(_config);
            _settingsWindow.ConfigChanged += (_, config) => ApplyConfig(config);
            _settingsWindow.Closed += (_, __) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        private void ApplyConfig(OverlayConfig config)
        {
            _config = config.Normalize();
            _overlay.UpdateConfig(_config);
            ConfigStore.Save(_configPath, _config);
        }

        private void ExitApplication()
        {
            _trayIcon.Visible = false;
            _settingsWindow?.Close();
            _hotkeyWindow.Dispose();
            _overlay.Close();
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
