using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Drawing.Point;

namespace AutoCall.WPF.Helpers {
    public static class PositionHelper {

        #region Types

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public enum ShowWindowCommands : int {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        private struct Rect {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        #endregion

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref System.Drawing.Rectangle rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        private static WINDOWPLACEMENT GetPlacement(IntPtr hwnd) {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        public static IntPtr GetindowPtr(string processName) {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Any()) {
                Process proc = processes[0];
                return proc.MainWindowHandle;
            } else {
                return IntPtr.Zero;
            }
        }

        public static ShowWindowCommands GetWindowState(IntPtr ptr) {
            WINDOWPLACEMENT placement = GetPlacement(ptr);
            return placement.showCmd;
        }

        public static Point GetWindowPosition(IntPtr ptr) {
            var rect = new System.Drawing.Rectangle();
            GetWindowRect(ptr, ref rect);
            return new Point(rect.X, rect.Y);
        }

        public static void SetWindowPosition(IntPtr ptr, Point position) {
            SetWindowPos(ptr, IntPtr.Zero, (int)position.X, (int)position.Y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }
    }
}
