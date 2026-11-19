using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class EmailListDialog : Window {

        public SendEmailViewModel Model {
            get { return DataContext as SendEmailViewModel; }
            set { DataContext = value; }
        }

        public EmailListDialog() {
            InitializeComponent();
        }

        public EmailListDialog(string message, string title, List<string> emails) {
            InitializeComponent();
            Title = title;
            Model = new SendEmailViewModel() {
                Text = message,
                EmailList = emails
            };
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            var list = Model.Emails.Where(e => e.Item2).Select(e => e.Item).ToList();
            // Cleans
            for (int i = list.Count - 1; i >= 0; i--) {
                if (String.IsNullOrWhiteSpace(list[i])) {
                    list.Remove(list[i]);
                    continue;
                }
                if (list[i].Trim() != list[i]) {
                    list[i] = list[i].Trim();
                }
            }
            if (list.Any() == false) {
                MessageBoxEx.Alert(this, "Please enter an email");
                return;
            }
            foreach (var item in list) {
                if (StringExtension.IsValidEmail(item) == false) {
                    MessageBoxEx.Alert(this, $"The email is not valid: '{item}'");
                    return;
                }
            }

            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void Add_OnClicked(object sender, RoutedEventArgs e) {
            Model.Emails.Add(new Wrapper2<string, bool>(String.Empty, true));
        }

        private void lbEmailsItem_OnLoaded(object sender, RoutedEventArgs e) {
            ((TextBox)sender).Focus();
        }
    }
}
