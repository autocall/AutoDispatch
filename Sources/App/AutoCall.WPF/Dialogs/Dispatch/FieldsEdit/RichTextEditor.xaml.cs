using System.Windows;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class RichTextEditor : Window {

        public string ViewModel {
            get {
                return xamlDataProvider.Xaml;
            }
            set {
                xamlDataProvider.Xaml = value;
            }
        }

        public RichTextEditor() {
            InitializeComponent();
        }

        private void RichTextEditor_OnLoaded(object sender, RoutedEventArgs e) {
            this.radRichTextBoxRibbonUI.SelectedIndex = 0;
            this.richTextBox.ScaleFactor = new Size(1.2, 1.2);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
