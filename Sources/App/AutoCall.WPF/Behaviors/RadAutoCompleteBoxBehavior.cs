using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Behaviors {
    public static class RadAutoCompleteBoxBehavior {
        public static readonly DependencyProperty TripleClickSelectAllProperty = DependencyProperty.RegisterAttached(
            "TripleClickSelectAll", typeof(bool), typeof(RadAutoCompleteBoxBehavior), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var tb = d as RadAutoCompleteBox;
            if (tb != null) {
                var enable = (bool)e.NewValue;
                if (enable) {
                    tb.PreviewMouseLeftButtonDown += OnRadAutoCompleteBoxMouseDown;
                } else {
                    tb.PreviewMouseLeftButtonDown -= OnRadAutoCompleteBoxMouseDown;
                }
            }
        }

        private static void OnRadAutoCompleteBoxMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 3) {
                var autoComplete = (RadAutoCompleteBox)sender;
                var textBox = autoComplete.ChildrenOfType<TextBox>().First();
                textBox.SelectAll();
            }
        }

        public static void SetTripleClickSelectAll(DependencyObject element, bool value) {
            element.SetValue(TripleClickSelectAllProperty, value);
        }

        public static bool GetTripleClickSelectAll(DependencyObject element) {
            return (bool)element.GetValue(TripleClickSelectAllProperty);
        }
    }
}
