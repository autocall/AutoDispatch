using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoCall.WPF.Extensions {
    public class EditableComboBox {
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(EditableComboBox), new PropertyMetadata(OnMaxLengthChanged));

        public static int GetMaxLength(DependencyObject obj) {
            return (int)obj.GetValue(MaxLengthProperty);
        }

        public static void SetMaxLength(DependencyObject obj, int value) {
            obj.SetValue(MaxLengthProperty, value);
        }

        private static async void OnMaxLengthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var comboBox = obj as Telerik.Windows.Controls.RadComboBox;
            if (comboBox == null)
                return;

            await comboBox.Dispatcher.InvokeAsync((Action)(() => {
                var rootElement = VisualTreeHelper.GetChild(comboBox, 0) as FrameworkElement;
                TextBox textBox = (TextBox)rootElement.FindName("PART_EditableTextBox");
                if (textBox != null)
                    textBox.SetValue(TextBox.MaxLengthProperty, e.NewValue);
            }));
        }
    }
}
