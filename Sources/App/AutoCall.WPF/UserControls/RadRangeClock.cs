using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    /// <summary>
    ///     V1 брал FindVisualAncestor<T> из Xceed.Wpf.AvalonDock.Controls (Extended WPF Toolkit, не переносится) — тот же обход визуальных родителей </summary>
    internal static class RadRangeClockExtensions {
        public static Grid FindVisualAncestorGrid(this DependencyObject dependencyObject) {
            DependencyObject target = dependencyObject;
            do {
                target = System.Windows.Media.VisualTreeHelper.GetParent(target);
            } while (target != null && (target is Grid) == false);
            return target as Grid;
        }
    }

    public class RadRangeClock : RadClock {

        #region DependencyProperty

        public static readonly DependencyProperty SelectedItem2Property =
            DependencyProperty.Register("SelectedItem2", typeof(TimeSpan?), typeof(RadRangeClock), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged)));

        public TimeSpan? SelectedItem2 {
            get { return (TimeSpan?)GetValue(SelectedItem2Property); }
            set { SetValue(SelectedItem2Property, value); }
        }

        #endregion DependencyProperty

        #region .ctor

        static RadRangeClock() {
            SelectedItemProperty.OverrideMetadata(typeof(RadRangeClock),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged)));
        }

        public RadRangeClock() {
            SelectionMode = SelectionMode.Extended;
        }

        #endregion .ctor

        #region Event Handler

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var SelectedItem2 = ((RadRangeClock)d).SelectedItem2;
            var SelectedItem = ((RadRangeClock)d).SelectedItem;
            var ItemsSource = ((RadRangeClock)d).ItemsSource;
            var SelectedItems = ((RadRangeClock)d).SelectedItems;

            if (SelectedItem != null && SelectedItem2 != null) {
                var times = ItemsSource.Cast<TimeSpan>();
                var SelectedItem1 = (TimeSpan)SelectedItem;

                foreach (var time in times) {
                    if ((SelectedItem1 <= SelectedItem2 && SelectedItem1 <= time && time <= SelectedItem2) ||
                        (SelectedItem2 <= SelectedItem1 && SelectedItem2 <= time && time <= SelectedItem1)) {
                        if (SelectedItems.Contains(time) == false) {
                            SelectedItems.Add(time);
                        }
                    } else {
                        if (SelectedItems.Contains(time)) {
                            SelectedItems.Remove(time);
                        }
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) {
                var grid = ((FrameworkElement)e.OriginalSource).FindVisualAncestorGrid();
                if (grid.DataContext is TimeSpan) {
                    SelectedItem2 = (TimeSpan)grid.DataContext;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            var grid = ((FrameworkElement)e.OriginalSource).FindVisualAncestorGrid();
            if (grid.DataContext is TimeSpan) {
                SelectedItem2 = (TimeSpan)grid.DataContext;
            }
            base.OnMouseUp(e);
        }

        #endregion Event Handler
    }
}
