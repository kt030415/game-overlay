using System.Text.RegularExpressions;

namespace GameOverlay.Core
{
    public sealed class OverlayConfig
    {
        private static readonly Regex HexColor = new Regex("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        public bool OverlayEnabled { get; set; } = true;
        public string ToggleHotkey { get; set; } = "Ctrl+Alt+X";
        public string LineColor { get; set; } = "#ffff00";
        public double LineOpacity { get; set; } = 0.35;
        public int LineThickness { get; set; } = 28;
        public int HorizontalLineLength { get; set; } = 360;
        public int VerticalLineLength { get; set; } = 320;
        public int CenterGap { get; set; } = 52;
        public int CenterReticleLength { get; set; } = 34;
        public int CenterReticleThickness { get; set; } = 6;
        public double CenterReticleOpacity { get; set; } = 0.9;
        public bool StretchLinesToEdges { get; set; } = true;

        public static OverlayConfig CreateDefault()
        {
            return new OverlayConfig();
        }

        public OverlayConfig Normalize()
        {
            OverlayConfig defaults = CreateDefault();
            string normalizedLineColor = HexColor.IsMatch(LineColor ?? string.Empty) ? LineColor! : defaults.LineColor;

            return new OverlayConfig
            {
                OverlayEnabled = OverlayEnabled,
                ToggleHotkey = string.IsNullOrWhiteSpace(ToggleHotkey) ? defaults.ToggleHotkey : ToggleHotkey,
                LineColor = normalizedLineColor,
                LineOpacity = ClampOpacity(LineOpacity, defaults.LineOpacity),
                LineThickness = Positive(LineThickness, defaults.LineThickness),
                HorizontalLineLength = Positive(HorizontalLineLength, defaults.HorizontalLineLength),
                VerticalLineLength = Positive(VerticalLineLength, defaults.VerticalLineLength),
                CenterGap = NonNegative(CenterGap, defaults.CenterGap),
                CenterReticleLength = Positive(CenterReticleLength, defaults.CenterReticleLength),
                CenterReticleThickness = Positive(CenterReticleThickness, defaults.CenterReticleThickness),
                CenterReticleOpacity = ClampOpacity(CenterReticleOpacity, defaults.CenterReticleOpacity),
                StretchLinesToEdges = StretchLinesToEdges
            };
        }

        private static double ClampOpacity(double value, double fallback)
        {
            return value >= 0 && value <= 1 ? value : fallback;
        }

        private static int Positive(int value, int fallback)
        {
            return value > 0 ? value : fallback;
        }

        private static int NonNegative(int value, int fallback)
        {
            return value >= 0 ? value : fallback;
        }
    }
}
