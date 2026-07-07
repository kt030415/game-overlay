using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GameOverlay.Core;

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
            Topmost = true;
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
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
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
