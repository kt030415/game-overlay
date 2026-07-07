using System;
using System.Windows.Forms;
using System.Windows.Interop;

namespace GameOverlay.App
{
    internal sealed class HotkeyWindow : IDisposable
    {
        private const int ModAlt = 0x0001;
        private const int ModControl = 0x0002;
        private const int HotkeyId = 9001;
        private readonly HwndSource _source;
        private bool _registered;

        public HotkeyWindow()
        {
            var parameters = new HwndSourceParameters("GameOverlayHotkeyWindow")
            {
                Width = 0,
                Height = 0,
                WindowStyle = 0
            };
            _source = new HwndSource(parameters);
            _source.AddHook(WndProc);
        }

        public event EventHandler? HotkeyPressed;

        public bool RegisterToggleHotkey()
        {
            _registered = Win32.RegisterHotKey(_source.Handle, HotkeyId, ModControl | ModAlt, (int)Keys.X);
            return _registered;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_registered)
            {
                Win32.UnregisterHotKey(_source.Handle, HotkeyId);
            }

            _source.RemoveHook(WndProc);
            _source.Dispose();
        }
    }
}
