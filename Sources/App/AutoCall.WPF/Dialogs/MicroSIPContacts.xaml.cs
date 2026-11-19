using AutoCall.Logic.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.Dialogs {
    public partial class MicroSIPContacts : Window {

        #region [ Properties ]

        public MicroSIPSettingsViewModel Model {
            get { return DataContext as MicroSIPSettingsViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        public MicroSIPContacts() {
            InitializeComponent();
        }

        #endregion [ .ctor ]

        #region [ Events ]

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i < this.Model.Contacts.Count; i++) {
                var shortcut = this.Model.Contacts[i];
                shortcut.Index = i + 1;
            }
        }

        private void btnAdd_OnClicked(object sender, RoutedEventArgs e) {
            this.Model.Contacts.Add(new MicroSIPContactViewModel() { Index = this.Model.Contacts.Count + 1 });
        }

        private void btnRemove_Clicked(object sender, RoutedEventArgs e) {
            this.Model.Contacts.Remove((MicroSIPContactViewModel)((Button)sender).DataContext);
            Window_Loaded(sender, e);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            bool flag = false;
            foreach (var shortcut in Model.Contacts) {

                if (String.IsNullOrEmpty(shortcut.Name)) {
                    shortcut.HasErrorName = true;
                    flag = true;
                } else {
                    shortcut.HasErrorName = false;
                }

                if (String.IsNullOrEmpty(shortcut.Number)) {
                    shortcut.HasErrorNumber = true;
                    flag = true;
                } else {
                    shortcut.HasErrorNumber = false;
                }

            }
            if (flag == true) {
                return;
            }
            DialogResult = true;
            Close();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        #endregion [ Events ]
    }
}
