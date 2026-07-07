using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GameOverlay.Core;

namespace GameOverlay.App
{
    internal sealed class SettingsWindow : Window
    {
        private TextBlock? _opacityValue;
        private OverlayConfig _config;

        public SettingsWindow(OverlayConfig config)
        {
            _config = config.Normalize();
            Title = "Game Overlay";
            Width = 300;
            Height = 150;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = false;
            Topmost = true;

            var root = new StackPanel
            {
                Margin = new Thickness(14)
            };

            root.Children.Add(BuildColorBar());
            root.Children.Add(BuildOpacityControl());
            Content = root;
        }

        public event EventHandler<OverlayConfig>? ConfigChanged;

        private UIElement BuildColorBar()
        {
            var bar = new UniformGrid
            {
                Columns = 5,
                Margin = new Thickness(0, 0, 0, 14)
            };

            AddColorButton(bar, "红", OverlayColorPresets.Red);
            AddColorButton(bar, "黄", OverlayColorPresets.Yellow);
            AddColorButton(bar, "蓝", OverlayColorPresets.Blue);
            AddColorButton(bar, "绿", OverlayColorPresets.Green);
            AddColorButton(bar, "青", OverlayColorPresets.Cyan);
            return bar;
        }

        private void AddColorButton(Panel parent, string name, string hex)
        {
            var button = new Button
            {
                Height = 28,
                Margin = new Thickness(3),
                ToolTip = name,
                Background = new SolidColorBrush(ParseColor(hex)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1)
            };
            button.Click += (_, __) => ApplyColor(hex);
            parent.Children.Add(button);
        }

        private UIElement BuildOpacityControl()
        {
            var panel = new DockPanel();
            _opacityValue = new TextBlock
            {
                Width = 48,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Text = FormatOpacity(_config.LineOpacity)
            };

            var label = new TextBlock
            {
                Width = 54,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "透明度"
            };

            var slider = new Slider
            {
                Minimum = 0.1,
                Maximum = 1.0,
                Value = _config.LineOpacity,
                TickFrequency = 0.05,
                IsSnapToTickEnabled = true,
                VerticalAlignment = VerticalAlignment.Center
            };
            slider.ValueChanged += (_, args) => ApplyOpacity(args.NewValue);

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(_opacityValue, Dock.Right);
            panel.Children.Add(label);
            panel.Children.Add(_opacityValue);
            panel.Children.Add(slider);
            return panel;
        }

        private void ApplyColor(string hex)
        {
            _config.LineColor = hex;
            PublishConfig();
        }

        private void ApplyOpacity(double opacity)
        {
            double rounded = Math.Round(opacity, 2);
            _config.LineOpacity = rounded;
            _config.CenterReticleOpacity = Math.Min(1.0, rounded + 0.35);
            if (_opacityValue != null)
            {
                _opacityValue.Text = FormatOpacity(rounded);
            }
            PublishConfig();
        }

        private void PublishConfig()
        {
            _config = _config.Normalize();
            ConfigChanged?.Invoke(this, _config);
        }

        private static string FormatOpacity(double opacity)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0}%", opacity * 100);
        }

        private static Color ParseColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            return Color.FromRgb(r, g, b);
        }
    }
}
