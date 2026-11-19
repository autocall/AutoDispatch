using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditContactsDialog : Window {
        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public ContactsViewModel ViewModel {
            get { return DataContext as ContactsViewModel; }
            set { DataContext = value; }
        }

        public EditContactsDialog() {
            InitializeComponent();
        }

        private void tbContact_OnLoaded(object sender, RoutedEventArgs e) {
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Add_OnClicked(object sender, RoutedEventArgs e) {
            var current = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var win = new ContactAddQuestion();
            if (current != null) {
                win.Owner = current;
            }
            if (win.ShowDialog() == true) {

                ContactBaseViewModel contact;
                switch (win.ContactType) {
                    case ContactType.Phone:
                        contact = new ContactPhoneViewModel();
                        break;
                    case ContactType.Fax:
                        contact = new ContactFaxViewModel();
                        break;
                    case ContactType.Email:
                        contact = new ContactEmailViewModel();
                        break;
                    default:
                        throw new Exception("Undefined type of contact");
                }
                ViewModel.Contacts.Add(contact);
            }
        }

        private void Next_OnClicked(object sender, RoutedEventArgs e) {
            var contact = (ContactBaseViewModel)((FrameworkElement)sender).DataContext;
            ViewModel.Contacts.Remove(contact);
            ViewModel.Contacts.Insert(0, contact);
        }

        private void Delete_OnClicked(object sender, RoutedEventArgs e) {
            var contact = (ContactBaseViewModel)((FrameworkElement)sender).DataContext;
            if (MessageBox.Show("Please confirm to delete contact\n" + contact.ToString(), "Delete contact", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }
            ViewModel.Contacts.Remove(contact);
        }
    }
}
