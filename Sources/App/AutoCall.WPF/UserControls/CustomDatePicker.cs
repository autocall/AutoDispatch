using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoCall.WPF.UserControls {
    public class CustomDatePicker : DatePicker {
        public CustomDatePicker() {
            Loaded += DatePicker_Loaded;
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e) {
            DatePicker datePicker = sender as DatePicker;
            Button button = datePicker.Template.FindName("PART_Button", datePicker) as Button;
            if (datePicker.IsEnabled == false) {
                (datePicker.Template.FindName("PART_TextBox", datePicker) as TextBox).Background = (Brush)(new BrushConverter()).ConvertFrom("#D7D7D7");
                (datePicker.Template.FindName("PART_TextBox", datePicker) as TextBox).Foreground = (Brush)(new BrushConverter()).ConvertFrom("#000000");
                button.Opacity = 1;
            } 
        }
    }
}
