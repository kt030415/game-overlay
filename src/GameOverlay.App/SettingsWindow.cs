using System;
using System.Collections.Generic;
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
        private CheckBox? _stretchCheckBox;
        private readonly Dictionary<string, Slider> _sliders = new Dictionary<string, Slider>();
        private readonly Dictionary<string, TextBlock> _sliderValues = new Dictionary<string, TextBlock>();
        private OverlayConfig _config;
        private bool _updatingControls;

        public SettingsWindow(OverlayConfig config)
        {
            _config = config.Normalize();
            Title = "Game Overlay";
            Width = 420;
            Height = 560;
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
            root.Children.Add(BuildStretchControl());
            root.Children.Add(BuildSlider("LineThickness", "线宽", 4, 48, _config.LineThickness, value => _config.LineThickness = value));
            root.Children.Add(BuildSlider("CenterGap", "中心间隙", 0, 400, _config.CenterGap, value => _config.CenterGap = value));
            root.Children.Add(BuildSlider("CenterReticleLength", "准星长度", 4, 80, _config.CenterReticleLength, value => _config.CenterReticleLength = value));
            root.Children.Add(BuildSlider("CenterReticleThickness", "准星宽度", 1, 24, _config.CenterReticleThickness, value => _config.CenterReticleThickness = value));

            Content = new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
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
            var panel = new DockPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
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
                if (!_updatingControls)
                {
                    PublishConfig();
                }
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

        private void ApplyStretch(bool stretch)
        {
            if (_updatingControls)
            {
                return;
            }

            _config.StretchLinesToEdges = stretch;
            PublishConfig();
        }

        private void RefreshControls()
        {
            _updatingControls = true;
            try
            {
                if (_stretchCheckBox != null)
                {
                    _stretchCheckBox.IsChecked = _config.StretchLinesToEdges;
                }

                SetSlider("LineThickness", _config.LineThickness);
                SetSlider("CenterGap", _config.CenterGap);
                SetSlider("CenterReticleLength", _config.CenterReticleLength);
                SetSlider("CenterReticleThickness", _config.CenterReticleThickness);
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void SetSlider(string key, int value)
        {
            Slider? slider;
            if (_sliders.TryGetValue(key, out slider))
            {
                slider.Value = value;
            }

            TextBlock? label;
            if (_sliderValues.TryGetValue(key, out label))
            {
                label.Text = value.ToString(CultureInfo.InvariantCulture);
            }
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
