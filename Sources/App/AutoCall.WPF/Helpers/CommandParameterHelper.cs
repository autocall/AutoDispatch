using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace AutoCall.WPF.Helpers {
    public static partial class UIHelper {
        public static DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
               "CommandParameter",
               typeof(object),
               typeof(UIHelper),
               new PropertyMetadata(CommandParameter_Changed));

        private static void CommandParameter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            {
                var target = d as ButtonBase;
                if (target != null) {
                    target.CommandParameter = e.NewValue;
                    var temp = target.Command;
                    // Have to set it to null first or CanExecute won't be called.
                    target.Command = null;
                    target.Command = temp;
                    return;
                }
            }
            {
                var target = d as Hyperlink;
                if (target != null) {
                    target.CommandParameter = e.NewValue;
                    var temp = target.Command;
                    // Have to set it to null first or CanExecute won't be called.
                    target.Command = null;
                    target.Command = temp;
                    return;
                }
            }
        }

        public static object GetCommandParameter(DependencyObject target) {
            return target.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject target, object value) {
            target.SetValue(CommandParameterProperty, value);
        }
    }
}
