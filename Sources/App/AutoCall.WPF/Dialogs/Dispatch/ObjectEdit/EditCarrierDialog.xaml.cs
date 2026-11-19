using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Telerik.Windows.Controls;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using AutoCall.WPF.Helpers;
using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoDispatch.Data;
using AutoCall.Common.Enums;
using AutoDispatch.Logic.Converters;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditCarrierDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;
        private DatCompanyDataService _dataService;

        private DatCompanyViewModel _originViewModel;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DatCompanyViewModel Model {
            get { return DataContext as DatCompanyViewModel; }
            set {
                DataContext = value;
                _originViewModel = (DatCompanyViewModel)value.Clone();
            }
        }

        private string _email;

        /// <summary>
        ///     Constructor </summary>
        public EditCarrierDialog(
            DispatchService service,
            DatCompanyViewModel company,
            ActualCompanyViewModel actualCompany,
            IList<ActualCompanyViewModel> actualCompanies,
            string email = null) {
            InitializeComponent();

            _email = email?.ToLower();
            _service = service;
            _dataService = App.Services.GetRequiredService<DatCompanyDataService>();

            if (company != null) {
                Model = company;
                Title = "Edit Carrier";
                tbMc.Text = company.McNumber?.ToString();
                tbDot.Text = company.DotNumber?.ToString();
                tbLegalName.Text = company.Name;
                SearchClicked(null, null);
                if (company.DoNotUse) {
                    var session = UserSession.Current;
                    if (RolesExtension.CheckPermission((RoleName)session.Roles, RoleName.Owner) == false) {
                        dnuCheckBox.IsEnabled = false;
                        ToolTipService.SetToolTip(dnuCheckBox, "The operation is allowed only to the owner");
                        ToolTipService.SetShowOnDisabled(dnuCheckBox, true);
                    }
                }
            } else {
                Model = new DatCompanyViewModel();
                Model.ActualCompanyId = actualCompany?.EntityId;
                Title = "Find Carrier";
            }
            Model.InsuranceExpirationChanged = false;

            cbActualCompany.IsEnabled = DispatchBoardViewModel.Instance.GetActualCompanyById(Model.ActualCompanyId) == null;
            cbActualCompany.ItemsSource = actualCompanies.Where(e => e.BoardType == DispatchBoardType.Broker).ToList();
        }

        private void EditDatCompanyDialog_OnSourceInitialized(object sender, EventArgs e) {
            filesControl.FilesLoad(_service, ErrorTextBlock, Model.EntityId);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            SaveDatCompany(w => SaveCompleted(w));
        }

        private void SaveDatCompany(Func<ViewModelWrapper<DatCompanyViewModel>, Task> completed) {
            if (Model.ActualCompanyId == null) {
                MessageBoxEx.Alert(this, "Please select a Dispatch Company");
                return;
            }

            DatCompanyType type;

            #region autonomous saving company
            int mc, dot = 0;
            if (Model.Fields == null && (Int32.TryParse(tbMc.Text, out mc) == true || Int32.TryParse(tbDot.Text, out dot) == true) &&
                String.IsNullOrEmpty(tbLegalName.Text) == false &&
                String.IsNullOrEmpty(tbEntityType.Text) == false) {
                tbEntityType.Text = tbEntityType.Text.ToUpper().Trim();
                if (EnumExtensions.TryParseByDesctiption<DatCompanyType>(tbEntityType.Text.ToUpper(), out type) == false) {
                    MessageBoxEx.Error(this, "unknown type of carrier: " + tbEntityType.Text);
                    return;
                }
                if (MessageBox.Show("please confirm you save the carrier without search by #?", "Find Carrier", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK) {
                    Model.EntityType = type;
                    Model.Name = tbLegalName.Text;
                    Model.McNumber = DispatchHelper.GetMc(mc);
                    Model.DotNumber = DispatchHelper.GetDot(dot);

                    _service.SaveDatCompany(Model, completed);

                    base.BusyStart();
                    return;
                }
            }
            #endregion autonomous saving company

            if (Model.Fields == null) {
                MessageBoxEx.Alert(this, "you have to find a carrier by # number");
                return;
            }
            if (Model.Fields.ContainsKey("Entity Type:") == false) {
                MessageBoxEx.Alert(this, "'Entity Type' was not found");
                return;
            }
            if (Model.Fields.ContainsKey("Legal Name:") == false) {
                MessageBoxEx.Alert(this, "'Legal Name' was not found");
                return;
            }
            var types = CarShipType.GetDatCompanyTypes(tbEntityType.Text);
            if (types.Any() == false) {
                MessageBoxEx.Error(this, "unknown type of carrier: " + tbEntityType.Text);
                return;
            } else {
                type = (DatCompanyType)types.Cast<int>().Sum();
            }

            Model.EntityType = type;
            Model.Name = Model.Fields["Legal Name:"];
            if (Model.Fields.ContainsKey("MC/MX/FF Number(s):") && String.IsNullOrEmpty(Model.Fields["MC/MX/FF Number(s):"]) == false) {
                try {
                    Model.McNumber = DispatchHelper.ParseMcNumber(Model.Fields["MC/MX/FF Number(s):"]);
                } catch (Exception) {
                }
            } else {
                if (Int32.TryParse(tbMc.Text, out mc)) {
                    Model.McNumber = DispatchHelper.GetMc(mc);
                }
            }

            if (Model.Fields.ContainsKey("USDOT Number:")) {
                Model.DotNumber = Int32.Parse(Model.Fields["USDOT Number:"]);
            } else {
                if (Int32.TryParse(tbDot.Text, out dot)) {
                    Model.DotNumber = DispatchHelper.GetDot(dot);
                }
            }

            _service.SaveDatCompany(Model, completed);

            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<DatCompanyViewModel> wrapper, bool close = true) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveCompleted(wrapper, close));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                Model.CreatedById = wrapper.Data.CreatedById;
                Model.CreatedDate = wrapper.Data.CreatedDate;
                Model.LastModifiedById = wrapper.Data.LastModifiedById;
                Model.LastModifiedDate = wrapper.Data.LastModifiedDate;

                if (Model.InsuranceFilePath != wrapper.Data.InsuranceFilePath) {
                    Model.InsuranceFilePath = wrapper.Data.InsuranceFilePath;
                }
            }
            await base.SaveCompleted(wrapper, close);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            // NOTE: The button has IsCanceled
            //DialogResult = false;
        }

        private async void SearchClicked(object sender, RoutedEventArgs args) {
            int? mc = null, dot = null;
            if (Int32.TryParse(tbMc.Text, out var tmp)) {
                mc = tmp;
            }
            if (Int32.TryParse(tbDot.Text, out tmp)) {
                dot = tmp;
            }
            if (mc != null || dot != null) {
                spSearchMc.IsEnabled = spSearchDot.IsEnabled = false;
                base.BusyStart();
                var result = await Task.Factory.StartNew(() => _dataService.SearchByMcDotNumber(
                    new SearchByMcDotNumberRequest() {
                        MC = mc,
                        DOT = dot
                    }
                ));
                spSearchMc.IsEnabled = spSearchDot.IsEnabled = true;
                base.BusyFinish();
                if (result.IsSuccessed) {
                    ErrorTextBlock.Visibility = Visibility.Hidden;
                    Model.Fields = result.Fields;
                    // NOTE: Shows color of "Operating Status:"
                    if (result.Fields.ContainsKey("color")) {
                        var color = ColorConverter.ConvertFromString(result.Fields["color"]);
                        if (color != null) {
                            tbOperatingStatus.Foreground = new SolidColorBrush((Color)color);
                        }
                    }

                    // Sets dot number
                    if (Model.Fields.ContainsKey("USDOT Number:") == true && Int32.TryParse(Model.Fields["USDOT Number:"], out var UsDotNumber)) {
                        if (Model.DotNumber != null && Model.DotNumber != UsDotNumber) {
                            Model.Contacts.Clear();
                            Debug.WriteLine($"DOT{UsDotNumber}, Contacts were Cleared");
                        }
                        Model.DotNumber = UsDotNumber;
                    }
                    if (Model.Fields.ContainsKey("MC/MX/FF Number(s):") == true && DispatchHelper.TryParseMcNumber(Model.Fields["MC/MX/FF Number(s):"], out var mcNumber)) {
                        Model.McNumber = mcNumber;
                    }
                    // Sets legal name
                    if (Model.Fields.ContainsKey("Legal Name:") == true) {
                        Model.Name = Model.Fields["Legal Name:"];
                    }

                    if (Model.Fields.ContainsKey("Phy_UpsStore") == true) {
                        Model.Phy_UpsStore = Model.Fields["Phy_UpsStore"] == true.ToString();
                    }
                    if (Model.Fields.ContainsKey("Mai_UpsStore") == true) {
                        Model.Mai_UpsStore = Model.Fields["Mai_UpsStore"] == true.ToString();
                    }
                    if (Model.Fields.ContainsKey("Phy_MailBox") == true) {
                        Model.Phy_MailBox = Model.Fields["Phy_MailBox"] == true.ToString();
                    }
                    if (Model.Fields.ContainsKey("Mai_MailBox") == true) {
                        Model.Mai_MailBox = Model.Fields["Mai_MailBox"] == true.ToString();
                    }

                    var TEL_NUM = Model.Fields.ContainsKey("Phone:") ? Model.Fields["Phone:"] : null;
                    var CELL_NUM = Model.Fields.ContainsKey("CELL_NUM") ? Model.Fields["CELL_NUM"] : null;
                    var FAX_NUM = Model.Fields.ContainsKey("FAX_NUM") ? Model.Fields["FAX_NUM"] : null;
                    var EMAILADDRESS = Model.Fields.ContainsKey("EMAILADDRESS") ? Model.Fields["EMAILADDRESS"] : null;
                    // NOTES: calculates in the Model
                    //var COMPANY_REP1 = Model.Fields.ContainsKey("COMPANY_REP1") ? Model.Fields["COMPANY_REP1"] : null;
                    //var COMPANY_REP2 = Model.Fields.ContainsKey("COMPANY_REP2") ? Model.Fields["COMPANY_REP2"] : null;

                    if (TEL_NUM != null && Model.Contacts.Contacts.Any(e => e is ContactPhoneViewModel phone && phone.Phone == TEL_NUM) == false) {
                        Model.Contacts.Add(PhoneContactConverter.ToVm(TEL_NUM, Model.ContactNames));
                    }
                    if (CELL_NUM != null && Model.Contacts.Contacts.Any(e => e is ContactPhoneViewModel phone && phone.Phone == CELL_NUM) == false) {
                        Model.Contacts.Add(PhoneContactConverter.ToVm(CELL_NUM));
                    }
                    if (FAX_NUM != null && Model.Contacts.Contacts.Any(e => e is ContactPhoneViewModel phone && phone.Phone == FAX_NUM) == false) {
                        Model.Contacts.Add(new ContactFaxViewModel() { Phone = ((ContactPhoneViewModel)PhoneContactConverter.ToVm(FAX_NUM)).Phone });
                    }
                    if (EMAILADDRESS != null && Model.Contacts.Contacts.Any(e => e is ContactEmailViewModel email && email.Email == EMAILADDRESS) == false) {
                        Model.Contacts.Add(new ContactEmailViewModel() { Email = EMAILADDRESS });
                    }
                    if (_email != null && Model.Contacts.Contacts.Any(e => e is ContactEmailViewModel email && email.Email == _email) == false) {
                        Model.Contacts.AddEmail(_email);
                    }
                    // sets entity type
                    if (Model.Fields.ContainsKey("Entity Type:") == true) {
                        var types = CarShipType.GetDatCompanyTypes(Model.Fields["Entity Type:"]);
                        if (types.Any()) {
                            Model.EntityType = (DatCompanyType)types.Cast<int>().Sum();
                        } else {
                            ErrorTextBlock.Visibility = Visibility.Visible;
                            ErrorTextBlock.Text = "unknown type of carrier: " + Model.Fields["Entity Type:"];
                        }
                    }
                } else {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    ErrorTextBlock.Text = result.ErrorMessage;
                }
            }
        }

        private void tbMcOrDot_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SearchClicked(null, null);
                e.Handled = true;
            }
        }

        private void InsuranceBrower_OnClicked(object sender, RoutedEventArgs e) {
            MessageBox.Show("Please confirm that your company is listed as certificate holder on carrier's insurance", "", MessageBoxButton.OK, MessageBoxImage.Information);

            var dialog = new OpenFileDialog();
            dialog.Title = "Attach insurance";
            if (dialog.ShowDialog() == true) {
                Model.InsuranceFilePath = Path.GetFileName(dialog.FileName);
                Model.InsuranceFileData = File.ReadAllBytes(dialog.FileName);
            }
        }

        private void Insurance_OnClick(object sender, RoutedEventArgs routedEventArgs) {
            if (String.IsNullOrEmpty(Model.InsuranceFilePath) == false) {
                InsuranceLoad(Model);
            } else {
                MessageBoxEx.Alert(this, "missing file");
            }
        }

        #region open

        private void InsuranceLoad(DatCompanyViewModel company) {
            BusyStart();
            imgShield.IsEnabled = false;
            _service.InsuranceLoad(company, InsuranceLoadCompeleted);
        }

        private async Task InsuranceLoadCompeleted(ViewModelWrapper<DatCompanyViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await InsuranceLoadCompeleted(wrapper));
                return;
            }
            imgShield.IsEnabled = true;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                try {
                    var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                    if (Directory.Exists(path) == false) {
                        Directory.CreateDirectory(path);
                    }
                    var fullPath = Path.Combine(path, wrapper.Data.InsuranceFilePath);
                    File.WriteAllBytes(fullPath, wrapper.Data.InsuranceFileData);
                    var process = new Process {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path }
                    };
                    process.Start();
                } catch (Exception exception) {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    if ((exception.HResult & 0xFFFF) == 0x0020) {
                        this.ErrorTextBlock.Text = $"File '{wrapper.Data.InsuranceFilePath}' already opened";
                    } else {
                        this.ErrorTextBlock.Text = exception.Message;
                    }
                }
            } else {
                ErrorTextBlock.Visibility = Visibility.Visible;
                this.ErrorTextBlock.Text = wrapper.MessageError;
            }
        }

        #endregion open

        #region [ setup packet ]

        private void SetupPacketLink_OnClick(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(new Uri(AutoCall.Common.Settings.WebUrl + $"CarrierRegistration/View/?CarrierId={Model.EntityId}").AbsoluteUri) { UseShellExecute = true });
        }

        private void SetupPacketEdit_OnClick(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(new Uri(AutoCall.Common.Settings.WebUrl + $"CarrierRegistration/Edit/?CarrierId={Model.EntityId}").AbsoluteUri) { UseShellExecute = true });
        }

        private void SetupPacketAccept_OnClick(object sender, RoutedEventArgs e) {
            if (Model.SetupPacket.WorkersCompensationInsuranceAgencyId != null) {
                if (Model.SetupPacket.WorkersCompensationInsuranceAgency != null) {
                    var tmp = Model.SetupPacket.WorkersCompensationInsuranceAgency;
                    var msgResult = MessageBox.Show($@"Insurance Agency Company Name: {tmp.CompanyName}
Insurance Agency Phone: {tmp.Phone}
Insurance Agency Fax: {tmp.Fax}
Insurance Agency Email: {tmp.Email}
Your Policy Number: {tmp.Number}

Are the insurance agency data entered correctly?", "Workers Compensation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.Yes) {
                        Model.SetupPacketAccept = true;
                        var datCompany = (DatCompanyViewModel)Model.Clone();
                        _service.SaveDatCompany(this.Model, ActionDatCompany_Completed);
                        BusyStart();
                    }
                    if (msgResult == MessageBoxResult.No) {
                        if (MessageBox.Show("Do you want to send an email to the carrier to edit the data of the insurance agency?",
                            "Workers Compensation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                            var datCompany = (DatCompanyViewModel)Model.Clone();
                            _service.SetupPacketEditSend(datCompany.EntityId, ActionDatCompany_Completed);
                            BusyStart();
                        }
                    }
                    if (msgResult == MessageBoxResult.Cancel) {
                        return;
                    }
                } else {
                    _webl.e("Model.SetupPacket.WorkersCompensationInsuranceAgency is null");
                    MessageBoxEx.Error(this, "Error");
                }
            } else if (MessageBox.Show("Please confirm to accept setup packet registration?", "setup packet registration", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                Model.SetupPacketAccept = true;
                var datCompany = (DatCompanyViewModel)Model.Clone();
                _service.SaveDatCompany(this.Model, ActionDatCompany_Completed);
                BusyStart();
            }
        }

        private void SetupPacketReject_OnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Please confirm the rejection of the setup packet registration?", "setup packet registration", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                Model.SetupPacketCompleted = false;
                var datCompany = (DatCompanyViewModel)Model.Clone();
                _service.SaveDatCompany(this.Model, ActionDatCompany_Completed);
                BusyStart();
            }
        }

        private void SetupPacketSend_OnClick(object sender, RoutedEventArgs e) {
            if (Model.ActualCompanyId == null) {
                MessageBoxEx.Alert(this, "Please select a Dispatch Company");
                return;
            }
            var emails = Model.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email).ToList();
            var datCompany = (DatCompanyViewModel)Model.Clone();
            if (datCompany.SetupPacketAccept == false && Model.SetupPacketCompleted == false) {
                var dialog = new EmailListDialog($"Please select email(s) to send invitation", "AutoCall - Send invitation of registration", emails.ToList());
                if (dialog.ShowDialog() == true) {
                    if (spSearchDot.IsEnabled && spSearchMc.IsEnabled) {
                        SaveDatCompany(w => SaveCompleted(w, false));
                    }
                    _service.SetupPacketInviteSend(datCompany.EntityId, dialog.Model.EmailList, ActionDatCompany_Completed);

                    // merges contacts
                    if (dialog.Model.SaveContacts) {
                        var contacts = new ContactsViewModel();
                        foreach (var email in dialog.Model.EmailList) {
                            contacts.AddEmail(email);
                        }
                        if (this.Model.Contacts.Merge(contacts)) {
                            // saves by OK
                        }
                    }
                    BusyStart();
                }
            }
        }

        private void SetupPacketSendV2_OnClick(object sender, RoutedEventArgs e) {
            if (Model.ActualCompanyId == null) {
                MessageBoxEx.Alert(this, "Please select a Dispatch Company");
                return;
            }
            var emails = Model.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email).ToList();
            var datCompany = (DatCompanyViewModel)Model.Clone();
            var dialog = new EmailListDialog($"Please select email(s) to send invitation", "AutoCall - Send invitation of registration", emails.ToList());
            if (dialog.ShowDialog() == true) {
                if (spSearchDot.IsEnabled && spSearchMc.IsEnabled) {
                    SaveDatCompany(w => SaveCompleted(w, false));
                }
                _service.SetupPacketInviteSend(datCompany.EntityId, dialog.Model.EmailList, ActionDatCompany_Completed);

                // merges contacts
                if (dialog.Model.SaveContacts) {
                    var contacts = new ContactsViewModel();
                    foreach (var email in dialog.Model.EmailList) {
                        contacts.AddEmail(email);
                    }
                    if (this.Model.Contacts.Merge(contacts)) {
                        // saves by OK
                    }
                }
                BusyStart();
            }
        }

        private async Task ActionDatCompany_Completed(ViewModelWrapper<DatCompanyViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await ActionDatCompany_Completed(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Model.AssignSetupPacketProperties(wrapper.Data);
            }
        }

        #endregion [ setup packet ]

        private void BaseDispatchDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (DialogResult != true && Model.HasChanges(this._originViewModel)) {
                var result = MessageBox.Show("Do you want to save the changes before closing?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result) {
                    case MessageBoxResult.Yes:
                        SaveDatCompany(w => SaveCompleted(w));
                        DialogResult = true;
                        e.Cancel = true;
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        DialogResult = null;
                        e.Cancel = true;
                        break;
                }
            }
        }

        #region [ Hyperlink ]

        private void OpenHyperlinkFile_OnClick(object sender, RoutedEventArgs e) {
            this.filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.HyperlinkFile
            });
        }

        private void UploadHyperlinkFile_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == true) {
                this.filesControl.FileSave(dialog.FileName);
                Model.HyperlinkFile = Path.GetFileName(dialog.FileName);
            }
        }

        private void RemoveHyperlinkFile_OnClick(object sender, RoutedEventArgs e) {
            Model.HyperlinkFile = null;
        }

        private void CarrierView_OnClick(object sender, RoutedEventArgs e) {
            var urlParameter = String.Empty;
            if (Model.DotNumber != null && Model.McNumber != null) {
                urlParameter = $"dot={Model.DotNumber}&docket={Model.McNumber}";
            } else if (Model.DotNumber != null && Model.McNumber == null) {
                urlParameter = $"dot={Model.DotNumber}";
            } else if (Model.DotNumber == null && Model.McNumber != null) {
                urlParameter = $"docket={Model.McNumber}";
            }
            var session = UserSession.Current;
            var ReturnUrl = $"https://brokersnapshot.com/SearchCompanies/Search?{urlParameter}";
            Process.Start(new ProcessStartInfo($"https://brokersnapshot.com/ExternalLogin?Token={session.AccessToken}&Type=1&ReturnUrl={Uri.EscapeDataString(ReturnUrl)}") { UseShellExecute = true });
        }

        #endregion [ Hyperlink ]

        #region [ clearable rating ]

        private double? _lastRating = null;
        private void RadRating_MouseDown(object sender, MouseButtonEventArgs e) {
            var rating = (RadRating)sender;
            _lastRating = rating.Value;
        }
        private void RadRating_MouseUp(object sender, MouseButtonEventArgs e) {
            var rating = (RadRating)sender;
            if (_lastRating == rating.Value) {
                rating.Value = 0;
                _lastRating = null;
            }
        }

        #endregion [ clearable rating ]
    }
}