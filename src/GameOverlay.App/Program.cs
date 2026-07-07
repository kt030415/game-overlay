namespace GameOverlay.App
{
    internal static class Program
    {
        [System.STAThread]
        private static void Main()
        {
            var app = new System.Windows.Application();
            app.Run(new OverlayWindow(GameOverlay.Core.OverlayConfig.CreateDefault()));
        }
    }
}
