using System.Windows;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    /// <summary>
    /// Interaction logic for ContactAddQuestion.xaml
    /// </summary>
    public partial class ContactAddQuestion : Window {

        public ContactType ContactType { get; set; }

        public ContactAddQuestion() {
            InitializeComponent();
        }

        private void btnPhone_Clicked(object sender, RoutedEventArgs e) {
            ContactType = ContactType.Phone;
            DialogResult = true;
        }

        private void btnFax_Clicked(object sender, RoutedEventArgs e) {
            ContactType = ContactType.Fax;
            DialogResult = true;
        }

        private void btnEmail_Clicked(object sender, RoutedEventArgs e) {
            ContactType = ContactType.Email;
            DialogResult = true;
        }
    }
}
