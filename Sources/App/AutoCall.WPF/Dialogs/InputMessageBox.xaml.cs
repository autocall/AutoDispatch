using System;
using System.Windows;

namespace AutoCall.WPF.Dialogs {
    public partial class InputMessageBox : Window {

        public InputMessageBox(string title, string description, string value) {
            InitializeComponent();

            Title = title;
            tbDescription.Text = description;
            tbText.Text = value;
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Close();
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(tbText.Text)) {
                MessageBox.Show("Please enter text");
                return;
            }
            this.DialogResult = false;
            Close();
        }
    }
}