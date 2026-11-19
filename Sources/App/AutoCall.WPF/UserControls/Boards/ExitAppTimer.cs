using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.UserControls.Boards {
    /// <summary>
    ///     «Are you here?» — закрытие App после простоя (Release — 7 дней). В V1 таймеры жили статикой в DatPowerBoard (модуль AutoCall,
    ///     не перенесён): статический конструктор запускал их при первом обращении, dispatch-борды перезапускали по кликам в гридах
    ///     (DatPowerBoard._exitAppTimer1.Stop/Start). Код V1 «#region to exit» без изменений; _stopTimer1 (остановка постингов DAT) не перенесён </summary>
    public static class ExitAppTimer {

        static ExitAppTimer() {
#if !DEBUG
            _exitAppTimer1.Tick += grids_ExitApp1;
            _exitAppTimer2.Tick += grids_ExitApp2;
            _exitAppTimer1.Stop(); _exitAppTimer1.Start();
#endif
        }

        /// <summary>
        ///     V1: DatPowerBoard._exitAppTimer1.Stop(); DatPowerBoard._exitAppTimer1.Start(); </summary>
        public static void Restart() {
            _exitAppTimer1.Stop(); _exitAppTimer1.Start();
        }

        #region [ to exit ]
#if DEBUG
        /// <summary>
        ///     The Time before alert </summary>
        public static DispatcherTimer _exitAppTimer1 = new DispatcherTimer() {
            Interval = TimeSpan.FromMinutes(15),
            IsEnabled = false
        };
        private static DateTime _exitTimer2Start = DateTime.MinValue;
        /// <summary>
        ///     The Time after alert. Every sec </summary>
        public static DispatcherTimer _exitAppTimer2 = new DispatcherTimer() {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = false
        };
        /// <summary>
        ///     Interval to closing </summary>
        private static TimeSpan _exitTimer2Interval = TimeSpan.FromSeconds(30);
#else
        /// <summary>
        ///     The Time before alert </summary>
        public static DispatcherTimer _exitAppTimer1 = new DispatcherTimer() {
            Interval = TimeSpan.FromDays(7),
            IsEnabled = false
        };
        private static DateTime _exitTimer2Start = DateTime.MinValue;
        /// <summary>
        ///     The Time after alert. Every sec </summary>
        public static DispatcherTimer _exitAppTimer2 = new DispatcherTimer() {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = false
        };
        /// <summary>
        ///     Interval to closing </summary>
        private static TimeSpan _exitTimer2Interval = TimeSpan.FromSeconds(10);
#endif
        /// <summary>
        ///     Alert control </summary>
        private static ContentControl _exitAppDialog;

        private static void grids_ExitApp1(object sender, EventArgs e) {
            Debug.WriteLine(" - grids_ExitPostings1");
            RadWindow.Alert(new DialogParameters() {
                Owner = App.Current.MainWindow,
                Header = "AutoDispatch",
                Content = $"Are you here?\nApplication will be closed in {(int)_exitTimer2Interval.TotalSeconds} seconds",
                Opened = (sender2, e2) => {
                    _exitAppDialog = (ContentControl)sender2;
                    _exitAppTimer1.Stop();
                    _exitAppTimer2.Start();
                    _exitTimer2Start = DateTime.UtcNow;
                },
                Closed = (sender3, e3) => {
                    // a close event before timeout
                    _exitAppTimer1.Start();
                    _exitAppTimer2.Stop();
                }
            });
        }

        private static void grids_ExitApp2(object sender, EventArgs e) {
            if (_exitTimer2Start != DateTime.MinValue) {
                var sec = (int)(_exitTimer2Interval - (DateTime.UtcNow - _exitTimer2Start)).TotalSeconds;
                if (sec < 0) sec = 0;
                ((ContentControl)_exitAppDialog.Content).Content = $"Are you here?\nApplication will be closed in {sec} seconds";
                if (sec == 0) {
                    _exitAppTimer1.Tick -= grids_ExitApp1;
                    _exitAppTimer2.Tick -= grids_ExitApp2;
                    _exitAppTimer1.Stop();
                    _exitAppTimer2.Stop();
                    Application.Current.MainWindow.Close();
                }
            }
        }

        #endregion [ to exit ]
    }
}
