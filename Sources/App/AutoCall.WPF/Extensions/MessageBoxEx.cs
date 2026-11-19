using System.Windows;

namespace AutoCall.WPF.Extensions {
    public static class MessageBoxEx {
        public static void Info(Window owner, string messge) {
            MessageBox.Show(owner, messge, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void Alert(Window owner, string messge) {
            MessageBox.Show(owner, messge, "Alert", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
        public static void Error(Window owner, string messge) {
            MessageBox.Show(owner, messge, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void Warning(Window owner, string messge) {
            MessageBox.Show(owner, messge, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Info(string messge) {
            MessageBox.Show(messge, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void Alert(string messge) {
            MessageBox.Show(messge, "Alert", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
        public static void Error(string messge) {
            MessageBox.Show(messge, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void Error(string messge, string caption) {
            MessageBox.Show(messge, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void Warning(string messge) {
            MessageBox.Show(messge, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static MessageBoxResult Question(string messge) {
            return MessageBox.Show(messge, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
