using System;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Documents.FormatProviders;

namespace AutoCall.WPF.Helpers {
    public static class UIElementHelper {
        public static T FindVisualParent<T>(this UIElement element) where T : UIElement {
            UIElement parent = element;
            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        public static T FindVisualParent2<T>(this UIElement element) where T : UIElement {
            UIElement parent = element;
            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }
                parent = (parent as FrameworkElement)?.Parent as UIElement;
            }
            return null;
        }

        public static EventHandler<SetupDocumentEventArgs> XamlDataProvider_OnSetupDocument => xamlDataProvider_OnSetupDocument;

        private static void xamlDataProvider_OnSetupDocument(object sender, SetupDocumentEventArgs e) {
            e.Document.LineSpacing = 1;
            e.Document.ParagraphDefaultSpacingAfter = 0.2;

        }
    }
}
