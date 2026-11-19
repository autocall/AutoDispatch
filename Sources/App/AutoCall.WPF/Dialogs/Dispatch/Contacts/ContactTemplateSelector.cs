using System.Windows;
using System.Windows.Controls;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public class ContactTemplateSelector: DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            FrameworkElement element = container as FrameworkElement;
            if (item == null) {
                return element.FindResource("NullTemplateKey") as DataTemplate;
            }
            if (item is ContactPhoneViewModel) {
                return element.FindResource("ContactPhoneTemplateKey") as DataTemplate;
            }
            if (item is ContactFaxViewModel) {
                return element.FindResource("ContactFaxTemplateKey") as DataTemplate;
            }
            if (item is ContactEmailViewModel) {
                return element.FindResource("ContactEmailTemplateKey") as DataTemplate;
            }
            return null;
        }
    }
}
