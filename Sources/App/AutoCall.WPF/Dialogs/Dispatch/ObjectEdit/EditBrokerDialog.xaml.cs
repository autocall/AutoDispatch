using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using AutoCall.WPF.Helpers;
using System.Collections.Generic;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Pages;
using System.Diagnostics;
using AutoDispatch.Data;
using AutoDispatch.Logic.Converters;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditBrokerDialog : BaseDispatchDialog {

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
        public EditBrokerDialog(
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
                Title = "Edit Broker";
                tbMc.Text = company.McNumber?.ToString();
                tbDot.Text = company.DotNumber?.ToString();
                tbLegalName.Text = company.Name;
                SearchClicked(null, null);
            } else {
                Model = new DatCompanyViewModel();
                Model.ActualCompanyId = actualCompany?.EntityId;
                Title = "Add Broker";
            }
            Model.InsuranceExpirationChanged = false;

            cbActualCompany.ItemsSource = actualCompanies.Where(e => e.BoardType == DispatchBoardType.Carrier).ToList();
            cbActualCompany.IsEnabled = DispatchBoardViewModel.Instance.GetActualCompanyById(Model.ActualCompanyId) == null;
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

            #region without mc

            if (String.IsNullOrEmpty(tbMc.Text) && String.IsNullOrEmpty(tbDot.Text)) {
                if (MessageBox.Show("please confirm you save the carrier without MC#?", "Add Carrier", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK) {
                    Model.EntityType = DatCompanyType.Broker;
                    Model.Name = tbLegalName.Text;

                    _service.SaveDatCompany(Model, completed);

                    base.BusyStart();
                    return;
                }
            }

            #endregion without mc

            #region autonomous saving company

            int mc, dot = 0;
            if (Model.Fields == null && (Int32.TryParse(tbMc.Text, out mc) == true || Int32.TryParse(tbDot.Text, out dot) == true) &&
                String.IsNullOrEmpty(tbLegalName.Text) == false &&
                String.IsNullOrEmpty(tbEntityType.Text) == false) {
                tbEntityType.Text = tbEntityType.Text.ToUpper().Trim();
                if (EnumExtensions.TryParseByDesctiption<DatCompanyType>(tbEntityType.Text, out type) == false) {
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
                MessageBoxEx.Alert(this, "you have to find a carrier by #");
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
            if (Model.Fields.ContainsKey("MC/MX/FF Number(s):") &&
                String.IsNullOrEmpty(Model.Fields["MC/MX/FF Number(s):"]) == false &&
                DispatchHelper.TryParseMcNumber(Model.Fields["MC/MX/FF Number(s):"], out var mcNumber)) {
                Model.McNumber = mcNumber;
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

        private async void SearchClicked(object sender, RoutedEventArgs e) {
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
                        Model.DotNumber = UsDotNumber;
                    }
                    if (Model.Fields.ContainsKey("MC/MX/FF Number(s):") == true && DispatchHelper.TryParseMcNumber(Model.Fields["MC/MX/FF Number(s):"], out var mcNumber)) {
                        Model.McNumber = mcNumber;
                    }
                    // Sets legal name
                    if (Model.Fields.ContainsKey("Legal Name:") == true) {
                        Model.Name = Model.Fields["Legal Name:"];
                    }
                    if (Model.Contacts.IsEmpty && Model.Fields.ContainsKey("Phone:")) {
                        var phone = Model.Fields["Phone:"];
                        Model.Contacts.Add(PhoneContactConverter.ToVm(phone));
                    }
                    if (_email != null) {
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