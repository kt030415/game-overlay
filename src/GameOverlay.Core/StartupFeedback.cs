namespace GameOverlay.Core
{
    public static class StartupFeedback
    {
        public const string HiddenOverlayTitle = "Game Overlay is running";
        public const string HiddenOverlayMessage = "Overlay is hidden. Press Ctrl+Alt+X or use the tray icon to show it.";

        public static bool ShouldShowHiddenOverlayNotice(OverlayConfig config)
        {
            return !config.Normalize().OverlayEnabled;
        }
    }
}
