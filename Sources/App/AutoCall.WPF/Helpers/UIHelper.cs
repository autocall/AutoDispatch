using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace AutoCall.WPF.Helpers {
    public static partial class UIHelper {

        public static readonly DependencyProperty TriggerProperty = DependencyProperty.RegisterAttached(
            "Trigger", typeof(object), typeof(UIHelper), new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(TriggerChanged)));

        private static void TriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            d.UpdateAllBindings();
        }

        public static void SetTrigger(DependencyObject element, object value) {
            element.SetValue(TriggerProperty, value);
        }

        public static object GetTrigger(DependencyObject element) {
            return element.GetValue(TriggerProperty);
        }

        public static void UpdateAllBindings(this DependencyObject o) {
            //Immediate Properties
            List<FieldInfo> propertiesAll = new List<FieldInfo>();
            Type currentLevel = o.GetType();
            while (currentLevel != typeof(object)) {
                propertiesAll.AddRange(currentLevel.GetFields());
                currentLevel = currentLevel.BaseType;
            }
            var propertiesDp = propertiesAll.Where(x => x.FieldType == typeof(DependencyProperty));
            foreach (var property in propertiesDp) {
                BindingExpression ex = BindingOperations.GetBindingExpression(o, (DependencyProperty)property.GetValue(o));
                if (ex != null)
                    ex.UpdateTarget();
            }

            //Children
            int childrenCount = VisualTreeHelper.GetChildrenCount(o);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(o, i);
                child.UpdateAllBindings();
            }
        }

        public static T GetDataContext<T>(object element) where T : class {
            if (element is FrameworkElement) {
                return (T)((FrameworkElement)element).DataContext;
            }
            if (element is FrameworkContentElement) {
                return (T)((FrameworkContentElement)element).DataContext;
            }
            throw new Exception($"Undefined Type of element: {element.GetType().Name}");
        }

        public static object GetDataContext(object element) {
            if (element is FrameworkElement) {
                return ((FrameworkElement)element).DataContext;
            }
            if (element is FrameworkContentElement) {
                return ((FrameworkContentElement)element).DataContext;
            }
            throw new Exception($"Undefined Type of element: {element.GetType().Name}");
        }

        public static string GetName(this DependencyObject element) {
            if (element is FrameworkElement) {
                return ((FrameworkElement)element).Name;
            }
            if (element is FrameworkContentElement) {
                return ((FrameworkContentElement)element).Name;
            }
            throw new Exception($"Undefined Type of element: {element.GetType().Name}");
        }

        public static string RecursiveSearchText(this TextBlock tb) {
            if (tb != null && String.IsNullOrEmpty(tb.Text) == false) {
                return tb.Text;
            }

            return _recursiveSearchText(tb);
        }

        private static string _recursiveSearchText(Object obj) {
            if (obj is Run) {
                return ((Run)obj).Text;
            }
            if (obj is Span || obj is TextBlock) {
                foreach (var inline in (obj as Span)?.Inlines ?? ((TextBlock)obj).Inlines) {
                    var result = _recursiveSearchText(inline);
                    if (String.IsNullOrEmpty(result)) {
                        continue;
                    } else {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
