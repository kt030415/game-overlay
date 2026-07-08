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
            Rectangle centerReticleHorizontal,
            Rectangle centerReticleVertical)
        {
            LeftLine = leftLine;
            RightLine = rightLine;
            TopLine = topLine;
            BottomLine = bottomLine;
            CenterReticleHorizontal = centerReticleHorizontal;
            CenterReticleVertical = centerReticleVertical;
        }

        public Rectangle LeftLine { get; }
        public Rectangle RightLine { get; }
        public Rectangle TopLine { get; }
        public Rectangle BottomLine { get; }
        public Rectangle CenterReticleHorizontal { get; }
        public Rectangle CenterReticleVertical { get; }
    }

    public static class OverlayGeometry
    {
        public static OverlayLayout Calculate(Size canvas, OverlayConfig config)
        {
            OverlayConfig normalized = config.Normalize();
            int centerX = canvas.Width / 2;
            int centerY = canvas.Height / 2;
            int halfThickness = normalized.LineThickness / 2;

            int reticleHalfLength = normalized.CenterReticleLength / 2;
            int reticleHalfThickness = normalized.CenterReticleThickness / 2;

            Rectangle centerReticleHorizontal = new Rectangle(
                centerX - reticleHalfLength,
                centerY - reticleHalfThickness,
                normalized.CenterReticleLength,
                normalized.CenterReticleThickness);

            Rectangle centerReticleVertical = new Rectangle(
                centerX - reticleHalfThickness,
                centerY - reticleHalfLength,
                normalized.CenterReticleThickness,
                normalized.CenterReticleLength);

            int leftEnd = centerX - normalized.CenterGap;
            int rightStart = centerX + normalized.CenterGap;
            int topEnd = centerY - normalized.CenterGap;
            int bottomStart = centerY + normalized.CenterGap;

            Rectangle leftLine = normalized.StretchLinesToEdges
                ? new Rectangle(0, centerY - halfThickness, leftEnd, normalized.LineThickness)
                : new Rectangle(
                    leftEnd - normalized.HorizontalLineLength,
                    centerY - halfThickness,
                    normalized.HorizontalLineLength,
                    normalized.LineThickness);

            Rectangle rightLine = normalized.StretchLinesToEdges
                ? new Rectangle(rightStart, centerY - halfThickness, canvas.Width - rightStart, normalized.LineThickness)
                : new Rectangle(
                    rightStart,
                    centerY - halfThickness,
                    normalized.HorizontalLineLength,
                    normalized.LineThickness);

            Rectangle topLine = normalized.StretchLinesToEdges
                ? new Rectangle(centerX - halfThickness, 0, normalized.LineThickness, topEnd)
                : new Rectangle(
                    centerX - halfThickness,
                    topEnd - normalized.VerticalLineLength,
                    normalized.LineThickness,
                    normalized.VerticalLineLength);

            Rectangle bottomLine = normalized.StretchLinesToEdges
                ? new Rectangle(centerX - halfThickness, bottomStart, normalized.LineThickness, canvas.Height - bottomStart)
                : new Rectangle(
                    centerX - halfThickness,
                    bottomStart,
                    normalized.LineThickness,
                    normalized.VerticalLineLength);

            return new OverlayLayout(leftLine, rightLine, topLine, bottomLine, centerReticleHorizontal, centerReticleVertical);
        }
    }
}
