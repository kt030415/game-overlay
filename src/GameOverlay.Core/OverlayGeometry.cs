using System.Drawing;

namespace GameOverlay.Core
{
    public sealed class OverlayLayout
    {
        public OverlayLayout(
            Rectangle leftLine,
            Rectangle rightLine,
            Rectangle topLine,
            Rectangle bottomLine,
            Rectangle centerPoint)
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
