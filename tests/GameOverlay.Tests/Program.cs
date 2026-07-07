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
            AssertEqual(26, config.CenterPointWidth, "center point width");
            AssertEqual(40, config.CenterPointHeight, "center point height");
            AssertEqual(0.9, config.CenterPointOpacity, "center point opacity");
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
                CenterPointWidth = 0,
                CenterPointHeight = 0,
                CenterPointOpacity = -1,
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

            AssertEqual(new Rectangle(535, 526, 360, 28), layout.LeftLine, "left line");
            AssertEqual(new Rectangle(1025, 526, 360, 28), layout.RightLine, "right line");
            AssertEqual(new Rectangle(946, 148, 28, 320), layout.TopLine, "top line");
            AssertEqual(new Rectangle(946, 612, 28, 320), layout.BottomLine, "bottom line");
            AssertEqual(new Rectangle(947, 520, 26, 40), layout.CenterPoint, "center point");
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
