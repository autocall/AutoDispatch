using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    /// <summary>
    ///     Interaction logic for LoadingControl.xaml </summary>
    public partial class LoadingControl : UserControl {
        #region [ Dependency Properties ]

        /// <summary>
        ///     Title dependency property </summary>
        public static readonly DependencyProperty LoadingMessageProperty =
            DependencyProperty.Register("LoadingMessage", typeof (string),
                typeof (LoadingControl), new PropertyMetadata(default(string), OnLoadingMessagePropertyChanged));

        #endregion [ Dependency Properties ]

        /// <summary>
        ///     Gets or sets loading message </summary>
        public string LoadingMessage {
            get { return (string) GetValue(LoadingMessageProperty); }
            set { SetValue(LoadingMessageProperty, value); }
        }

        public LoadingControl() {
            InitializeComponent();
        }

        private static void OnLoadingMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        }
    }
}