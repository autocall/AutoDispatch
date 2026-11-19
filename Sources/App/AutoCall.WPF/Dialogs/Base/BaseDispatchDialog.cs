using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Loggers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs {
    public abstract class BaseDispatchDialog : Window {
        protected abstract RadProgressBar ProgressBar { get; }
        protected abstract TextBlock ErrorTextBlock { get; }
        protected abstract Button OkButton { get; }
        protected abstract Button CloseButton { get; }

        private List<string> _errors = new List<string>();

        public BaseDispatchDialog() { 
            StackLogger.Push($"{this.GetType().Name}.ctor");
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            var rand = new Random();
            this.Left += rand.Next(50);
            this.Top += rand.Next(50);
        }

        public virtual void BusyStart(string message = null) {
            ProgressBar.Visibility = Visibility.Visible;
            if (OkButton != null) {
                OkButton.IsEnabled = false;
            }
            CloseButton.IsEnabled = false;
            ErrorTextBlock.Visibility = Visibility.Hidden;
            _errors.Clear();
            ErrorTextBlock.Text = String.Empty;
        }

        public virtual void BusyFinish() {
            ProgressBar.Visibility = Visibility.Collapsed;
            if (OkButton != null) {
                OkButton.IsEnabled = true;
            }
            CloseButton.IsEnabled = true;
        }

        public async Task SaveCompleted<T>(ViewModelWrapper<T> wrapper, bool close = true) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveCompleted(wrapper, close));
                return;
            }
            BusyFinish();
            if (wrapper.Status == ResponseStatus.Success) {
                if (this.DialogResult == null && close == true)
                    DialogResult = true;
            } else {
                ErrorTextBlock.Visibility = Visibility.Visible;
                _errors.Add(wrapper.MessageError);
                ErrorTextBlock.Text = String.Join("\n", _errors);
            }
        }

        public async Task SaveCompleted<T, F>(ViewModelWrapper<T> wrapper1, ViewModelWrapper<F> wrapper2, bool close = true) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveCompleted(wrapper1, wrapper2, close));
                return;
            }
            BusyFinish();

            if (wrapper1.Status == ResponseStatus.Success && wrapper2.Status == ResponseStatus.Success) {
                if (this.DialogResult == null && close == true)
                    DialogResult = true;
            } else {
                ErrorTextBlock.Visibility = Visibility.Visible;
                if (wrapper1.MessageError != null) {
                    _errors.Add(wrapper1.MessageError);
                }
                if (wrapper2.MessageError != null) {
                    _errors.Add(wrapper2.MessageError);
                }
                ErrorTextBlock.Text = String.Join("\n", _errors);
            }
        }
    }
}
