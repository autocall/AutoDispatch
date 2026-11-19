using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    public partial class RadRangeDateTimePicker : RadDateTimePicker {
        public RadRangeDateTimePicker() {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedTime2Property =
       DependencyProperty.Register("SelectedTime2", typeof(TimeSpan?), typeof(RadRangeDateTimePicker));

        public TimeSpan? SelectedTime2 {
            get { return (TimeSpan?)GetValue(SelectedTime2Property); }
            set { SetValue(SelectedTime2Property, value); }
        }

    }

    public class DateTemplateSelector : DataTemplateSelector {
        public DataTemplate DefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            return this.DefaultTemplate;
        }
    }
}
