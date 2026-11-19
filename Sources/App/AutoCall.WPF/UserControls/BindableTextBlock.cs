using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AutoCall.WPF.UserControls {
    /// <summary>
    ///     http://stackoverflow.com/questions/1959856/data-binding-the-textblock-inlines </summary>
    public class BindableTextBlock : TextBlock {
        public IEnumerable<Inline> InlineList {
            get { return (IEnumerable<Inline>) GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof (IEnumerable<Inline>), typeof (BindableTextBlock),
                new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var list = e.NewValue as IEnumerable<Inline>;
            if (list != null) {
                ((TextBlock) sender).Inlines.Clear();
                ((TextBlock) sender).Inlines.AddRange(list);
            }
        }
    }
}