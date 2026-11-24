using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class SendInvoiceDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        public SendEmailViewModel Model {
            get { return DataContext as SendEmailViewModel; }
            set { DataContext = value; }
        }

        public SendInvoiceDialog() {
            InitializeComponent();
        }

        public SendInvoiceDialog(List<string> emails, LoadViewModel load, FactoringCompanyViewModel factoringCompany) {
            InitializeComponent();
            Model = new SendEmailViewModel() {
                Load = load,
                EmailList = emails,
                SendInvoice = true,
            };
            if (factoringCompany.Email != null) {
                Model.FactoringCompanyEmail = new Wrapper2<string, bool>(factoringCompany.Email, true);
            } else {
                if (Model.Emails.Any() == false) {
                    Model.Emails.Add(new Wrapper2<string, bool>(String.Empty, true));
                }
            }
        }

        private bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            if (Model.SendConfirmation == false && Model.SendInvoice == false && Model.SendPOD == false && Model.SendLumper == false) {
                MessageBoxEx.Alert(this, "Please select an file");
                return;
            }
            List<string> emails = Model.Emails.Where(e => e.Item2).Select(e => e.Item).ToList();
            if (Model.FactoringCompanyEmail != null && Model.FactoringCompanyEmail.Item2 == true && Model.FactoringCompanyEmail.Item != null) {
                emails.Add(Model.FactoringCompanyEmail.Item);
            }
            // Cleans
            for (int i = emails.Count - 1; i >= 0; i--) {
                if (String.IsNullOrWhiteSpace(emails[i])) {
                    emails.Remove(emails[i]);
                    continue;
                }
                if (emails[i].Trim() != emails[i]) {
                    emails[i] = emails[i].Trim();
                }
            }
            if (emails.Any() == false) {
                MessageBoxEx.Alert(this, "Please enter an email");
                return;
            }
            foreach (var item in emails) {
                if (IsValidEmail(item) == false) {
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

        private void Invoice_OnClick(object sender, RoutedEventArgs e) {
            var model = new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.InvoiceFile
            };
            var service = App.Services.GetRequiredService<DispatchService>();
            service.FileLoad(model, FileLoadCompleted);
            base.BusyStart();
            ErrorTextBlock.Visibility = Visibility.Hidden;
        }

        private void POD_OnClick(object sender, RoutedEventArgs e) {
            var model = new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.PODFile
            };
            var service = App.Services.GetRequiredService<DispatchService>();
            service.FileLoad(model, FileLoadCompleted);
            base.BusyStart();
            ErrorTextBlock.Visibility = Visibility.Hidden;
        }

        private void Lumper_OnClick(object sender, RoutedEventArgs e) {
            var model = new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.LumperFile
            };
            var service = App.Services.GetRequiredService<DispatchService>();
            service.FileLoad(model, FileLoadCompleted);
            base.BusyStart();
            ErrorTextBlock.Visibility = Visibility.Hidden;
        }

        private void Confirmation_OnClick(object sender, RoutedEventArgs e) {
            var model = new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.ConfirmationFile
            };
            var service = App.Services.GetRequiredService<DispatchService>();
            service.FileLoad(model, FileLoadCompleted);
            base.BusyStart();
            ErrorTextBlock.Visibility = Visibility.Hidden;
        }

        private async Task FileLoadCompleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                if (wrapper.Status == ResponseStatus.Success) {
                    try {
                        var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                        if (Directory.Exists(path) == false) {
                            Directory.CreateDirectory(path);
                        }
                        var fullPath = Path.Combine(path, wrapper.Data.Name);
                        File.WriteAllBytes(fullPath, wrapper.Data.Data);
                        var process = new Process {
                            EnableRaisingEvents = true,
                            StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path, UseShellExecute = true }
                        };
                        process.Start();
                        base.BusyFinish();
                    } catch (Exception exception) {
                        this.ErrorTextBlock.Visibility = Visibility.Visible;
                        this.ErrorTextBlock.Text = exception.Message;
                        base.BusyFinish();
                    }
                } else {
                    this.ErrorTextBlock.Visibility = Visibility.Visible;
                    this.ErrorTextBlock.Text = wrapper.MessageError;
                    base.BusyFinish();
                }
            });
        }

        private void Merged_OnClick(object sender, RoutedEventArgs e) {
            var service = App.Services.GetRequiredService<DispatchService>();
            service.InvoiceMerge(Model.Load.EntityId, Model.SendInvoice, Model.SendPOD, Model.SendConfirmation, Model.SendLumper, FileLoadCompleted);
            base.BusyStart();
            ErrorTextBlock.Visibility = Visibility.Hidden;
        }
    }
}
