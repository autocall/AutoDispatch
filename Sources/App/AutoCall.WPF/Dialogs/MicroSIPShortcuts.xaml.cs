using AutoCall.Logic.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.Dialogs {
    public partial class MicroSIPShortcuts : Window {

        #region [ Properties ]

        public MicroSIPSettingsViewModel Model {
            get { return DataContext as MicroSIPSettingsViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        public MicroSIPShortcuts() {
            InitializeComponent();
        }

        #endregion [ .ctor ]

        #region [ Events ]

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i < this.Model.Shortcuts.Count; i++) {
                var shortcut = this.Model.Shortcuts[i];
                shortcut.Index = i + 1;
            }
        }

        private void btnAdd_OnClicked(object sender, RoutedEventArgs e) {
            this.Model.Shortcuts.Add(new MicroSIPShortcutViewModel() { Index = this.Model.Shortcuts.Count + 1 });
        }

        private void btnRemove_Clicked(object sender, RoutedEventArgs e) {
            this.Model.Shortcuts.Remove((MicroSIPShortcutViewModel)((Button)sender).DataContext);
            Window_Loaded(sender, e);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            bool flag = false;
            foreach (var shortcut in Model.Shortcuts) {

                if (String.IsNullOrEmpty(shortcut.Label)) {
                    shortcut.HasErrorLabel = true;
                    flag = true;
                } else {
                    shortcut.HasErrorLabel = false;
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
