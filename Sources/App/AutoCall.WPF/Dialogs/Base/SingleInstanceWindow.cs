using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AutoCall.WPF.Dialogs {
    internal class NativeMethods {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }

    // TODO: window doesn't clickable if exists open MessageBox
    public class SingleInstanceWindow : Window {
        public SingleInstanceWindow() {
            SourceInitialized += SingleInstanceWindow_SourceInitialized;
        }

        private void SingleInstanceWindow_SourceInitialized(object sender, EventArgs e) {
            SourceInitialized -= SingleInstanceWindow_SourceInitialized;

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == NativeMethods.WM_SHOWME) {
                if (WindowState == WindowState.Minimized) {
                    WindowState = WindowState.Normal;
                }
                // get our current "TopMost" value (ours will always be false though)
                bool top = Topmost;
                // make our form jump to the top of everything
                Topmost = true;
                // set it back to whatever it was
                Topmost = top;
            }

            return IntPtr.Zero;
        }
    }
}
