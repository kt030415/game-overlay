namespace GameOverlay.App
{
    internal static class Program
    {
        [System.STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            var app = new GameOverlayApplication();
            app.Run();
        }
    }
}
