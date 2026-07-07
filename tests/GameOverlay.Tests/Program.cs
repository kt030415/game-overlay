using System;
using System.Drawing;
using System.IO;
using GameOverlay.Core;

namespace GameOverlay.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                DefaultConfigMatchesReference();
                InvalidConfigFallsBackToDefaults();
                MissingConfigIsCreated();
                GeometryPlacesSegmentsAroundCenter();
                ColorPresetsProvideRequestedColors();
                Console.WriteLine("All tests passed.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void DefaultConfigMatchesReference()
        {
            OverlayConfig config = OverlayConfig.CreateDefault();
            AssertEqual("#ffff00", config.LineColor, "line color");
            AssertEqual(0.35, config.LineOpacity, "line opacity");
            AssertEqual(28, config.LineThickness, "line thickness");
            AssertEqual(360, config.HorizontalLineLength, "horizontal length");
            AssertEqual(320, config.VerticalLineLength, "vertical length");
            AssertEqual(52, config.CenterGap, "center gap");
            AssertEqual(34, config.CenterReticleLength, "center reticle length");
            AssertEqual(6, config.CenterReticleThickness, "center reticle thickness");
            AssertEqual(0.9, config.CenterReticleOpacity, "center reticle opacity");
            AssertEqual(true, config.StretchLinesToEdges, "stretch lines");
            AssertEqual("Ctrl+Alt+X", config.ToggleHotkey, "hotkey");
        }

        private static void InvalidConfigFallsBackToDefaults()
        {
            OverlayConfig invalid = new OverlayConfig
            {
                LineColor = "yellow",
                LineOpacity = 2,
                LineThickness = -1,
                HorizontalLineLength = 0,
                VerticalLineLength = 0,
                CenterGap = -10,
                CenterReticleLength = 0,
                CenterReticleThickness = 0,
                CenterReticleOpacity = -1,
                StretchLinesToEdges = true,
                ToggleHotkey = ""
            };

            OverlayConfig normalized = invalid.Normalize();
            AssertEqual("#ffff00", normalized.LineColor, "normalized line color");
            AssertEqual(28, normalized.LineThickness, "normalized line thickness");
            AssertEqual("Ctrl+Alt+X", normalized.ToggleHotkey, "normalized hotkey");
        }

        private static void MissingConfigIsCreated()
        {
            string dir = Path.Combine(Path.GetTempPath(), "game-overlay-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "config.json");
            OverlayConfig config = ConfigStore.LoadOrCreate(path);
            AssertEqual(true, File.Exists(path), "config file exists");
            AssertEqual(28, config.LineThickness, "created config thickness");
            Directory.Delete(dir, true);
        }

        private static void GeometryPlacesSegmentsAroundCenter()
        {
            OverlayConfig config = OverlayConfig.CreateDefault();
            OverlayLayout layout = OverlayGeometry.Calculate(new Size(1920, 1080), config);

            AssertEqual(new Rectangle(0, 526, 891, 28), layout.LeftLine, "left line");
            AssertEqual(new Rectangle(1029, 526, 891, 28), layout.RightLine, "right line");
            AssertEqual(new Rectangle(946, 0, 28, 471), layout.TopLine, "top line");
            AssertEqual(new Rectangle(946, 609, 28, 471), layout.BottomLine, "bottom line");
            AssertEqual(new Rectangle(943, 537, 34, 6), layout.CenterReticleHorizontal, "center reticle horizontal");
            AssertEqual(new Rectangle(957, 523, 6, 34), layout.CenterReticleVertical, "center reticle vertical");
        }

        private static void ColorPresetsProvideRequestedColors()
        {
            AssertEqual("#ff0000", OverlayColorPresets.Red, "red preset");
            AssertEqual("#ffff00", OverlayColorPresets.Yellow, "yellow preset");
            AssertEqual("#0066ff", OverlayColorPresets.Blue, "blue preset");
            AssertEqual("#00ff00", OverlayColorPresets.Green, "green preset");
            AssertEqual("#00ffff", OverlayColorPresets.Cyan, "cyan preset");
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!object.Equals(expected, actual))
            {
                throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
            }
        }
    }
}
