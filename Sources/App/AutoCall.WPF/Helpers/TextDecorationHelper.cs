using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.Helpers {
    public class TextDecorationHelper {
        public static TextDecorationCollection GetTextDecoration(DependencyObject obj) {
            return (TextDecorationCollection) obj.GetValue(TextDecorationProperty);
        }

        public static void SetTextDecoration(DependencyObject obj, TextDecorationCollection value) {
            obj.SetValue(TextDecorationProperty, value);
        }

        public static readonly DependencyProperty TextDecorationProperty =
            DependencyProperty.RegisterAttached("TextDecoration", typeof (TextDecorationCollection),
                typeof (TextDecorationHelper), new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits,
                    new PropertyChangedCallback((d, e) => {
                        TextBlock textBlock = d as TextBlock;
                        if (textBlock != null) {
                            textBlock.TextDecorations = e.NewValue as TextDecorationCollection;
                        }
                    })));
    }
}