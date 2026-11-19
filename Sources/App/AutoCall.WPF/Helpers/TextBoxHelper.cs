using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoCall.WPF.Helpers {
    public static class TextBoxHelper {
        public static TextCompositionEventHandler NumberValidationTextBox => _numberValidationTextBox;
        public static void _numberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
