using System;
using System.Windows;
using System.Windows.Input;

namespace AutoCall.WPF.Commands {
    /// <summary>
    ///     A command that occurs when icon in tray is clicked and pass command parameter as Window </summary>
    public class MaximizedWindowCommand : ICommand {
        public void Execute(object parameter) {
            var window = (Window) parameter;
            if (window.WindowState == WindowState.Minimized) {
                window.ShowInTaskbar = true;
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}