using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class ContactsControl : UserControl {

        private object _focusing = null;

        public ContactsViewModel ViewModel => root.DataContext as ContactsViewModel;

        public ContactsControl() {
            InitializeComponent();
        }

        private void ContactsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var oldValue = e.OldValue as ContactsViewModel;
            var newValue = e.NewValue as ContactsViewModel;
            if (oldValue != null) {
                oldValue.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (newValue != null) {
                newValue.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ContactsControl_OnLoaded(object sender, RoutedEventArgs e) {
            root.DataContextChanged += ContactsControl_DataContextChanged;
            if (ViewModel != null) {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ContactsControl_OnUnloaded(object sender, RoutedEventArgs e) {
            if (ViewModel != null) {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            root.GetBindingExpression(DataContextProperty)?.UpdateSource();
        }

        private void Next_OnClicked(object sender, RoutedEventArgs e) {
            ViewModel.Next();
        }

        private void Edit_OnClicked(object sender, RoutedEventArgs e) {
            var dialog = new EditContactsDialog();
            dialog.ViewModel = (ContactsViewModel)ViewModel.Clone();
            if (dialog.ShowDialog() == true) {
                ViewModel.Assign(dialog.ViewModel);
            }
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
                ViewModel.InsertToTop(contact);
                _focusing = contact;
            }
        }

        private void tbContact_OnLoaded(object sender, RoutedEventArgs e) {
            if (_focusing != null) {
                var item = (FrameworkElement)sender;
                if (item.DataContext == _focusing) {
                    item.Focus();
                    _focusing = null;
                }
            }
        }
    }
}
