using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoCall.WPF.UserControls {
    public class EnabledTextBlock : TextBlock {

        static EnabledTextBlock() {
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(EnabledTextBlock), new UIPropertyMetadata(true, IsEnabledChanged, CoerceIsEnabled));
        }

        private new static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        private static object CoerceIsEnabled(DependencyObject d, object basevalue) {
            var parent = VisualTreeHelper.GetParent(d) as FrameworkElement;
            if (parent != null) {
                if (parent.IsEnabled == false) {
                    return true;
                }
            }
            return basevalue;
        }
    }
}
