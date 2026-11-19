using AutoDispatch.Logic.Contracts.ViewModels;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Windows;
using AutoCall.Logic.Settings;
using System.Windows.Controls;
using System.Threading.Tasks;
using AutoMapper;
using AutoDispatch.Logic.DataServices;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Dtos.Dtos;
using AutoCall.Common;
using AutoDispatch.Dtos.Enums;
using System.Linq;
using System.Diagnostics;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Converters;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class CensusOfCompaniesDialog : Window {

        private DispatchService _dispatchService;
        private DatCompanyDataService _datCompanyService;
        private SettingsService _settings;
        private IMapper _mapper;

        private Task task = null;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public CensusOfCompaniesViewModel Model {
            get { return DataContext as CensusOfCompaniesViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Constructor </summary>
        public CensusOfCompaniesDialog(DatCompanyType companyType, IMapper mapper, SettingsService settings,
            ActualCompanyViewModel company, List<DatCompanyViewModel> datCompanies) {
            _mapper = mapper;
            _datCompanyService = App.Services.GetRequiredService<DatCompanyDataService>();
            _dispatchService = App.Services.GetRequiredService<DispatchService>();
            _settings = settings;

            InitializeComponent();

            Model = new CensusOfCompaniesViewModel() {
                CompanyType = companyType,
                DatCompanies = datCompanies,
                ActualCompany = company,
                Search = new CensusCompanySearchViewModel() {
                    Limit = 20,
                }
            };
            switch (companyType) {
                case DatCompanyType.Broker:
                    Title = "All Brokers";
                    Model.Search.CompanyType = DatCompanyType.Broker;
                    break;
                case DatCompanyType.Carrier:
                    Title = "All Carriers";
                    Model.Search.CompanyType = DatCompanyType.Carrier;
                    break;
                default:
                    throw new Exception();
            }
        }

        private async void Filter_TextChanged(object sender, TextChangedEventArgs e) {
            #region [ single request + delay ]
            var localTask = task = Task.Delay(1000);
            await task;
            if (localTask != task) {
                return;
            }
            #endregion [ single request + delay ]

            try {
                Model.Processing = true;

                // Get data from service
                var result = await _datCompanyService.SearchCensusCompaniesAsync(new SearchCensusCompaniesRequest() {
                    Search = _mapper.Map<CensusCompanySearchDto>(Model.Search)
                });

                if (result.IsSuccessed) {
                    // Populate autocomplete items source
                    Model.Companies = _mapper.Map<List<CensusCompanyViewModel>>(result.CensusCompanies);
                    Model.CalcSmallLogo();
                } else {
                    _l.e("Filter_TextChanged", result.ErrorMessage);
                }
            } finally {
                Model.Processing = false;
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void CreateBroker_OnClick(object sender, RoutedEventArgs arg) {
            EditComapny();
        }

        private void CreateCarrier_OnClick(object sender, RoutedEventArgs arg) {
            EditComapny();
        }

        private void gridCompanies_OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            EditComapny();
        }

        private void EditComapny() {
            if (gridCompanies.SelectedItem != null) {
                var censusCompany = (CensusCompanyViewModel)gridCompanies.SelectedItem;
                var datCompany = this.Model.DatCompanies.FirstOrDefault(e => e.DotNumber == censusCompany.DOT_NUMBER);
                if (datCompany == null) {
                    datCompany = new DatCompanyViewModel();
                    if (censusCompany.McNumber != null) {
                        datCompany.McNumber = censusCompany.McNumber;
                    } else {
                        datCompany.DotNumber = censusCompany.DOT_NUMBER;
                    }
                    datCompany.ActualCompanyId = Model.ActualCompany?.EntityId;
                }

                var entityTypes = new List<string>();
                if (censusCompany.CARSHIP.Contains("C")) {
                    entityTypes.Add("CARRIER");
                }
                if (censusCompany.CARSHIP.Contains("B")) {
                    entityTypes.Add("BROKER");
                }

                // SET FIELDS AND PROPERTIES FROM CENSUS ---------->
                datCompany.Fields = new Dictionary<string, string>() {
                    { "Entity Type:", String.Join("/", entityTypes) },
                    { "USDOT Number:", censusCompany.DOT_NUMBER.ToString() },
                    { "Legal Name:", censusCompany.NAME },
                    { "DBA Name:", censusCompany.NAME_DBA },
                    { "Physical Address:", $"{censusCompany.PHY_STR}\n{censusCompany.PHY_CITY}, {censusCompany.PHY_ST} {censusCompany.PHY_ZIP}" },
                    { "Mailing Address:", $"{censusCompany.MAI_STR}\n{censusCompany.MAI_CITY}, {censusCompany.MAI_ST} {censusCompany.MAI_ZIP}" },
                    { "Phone:", censusCompany.TEL_NUM },
                    //{ "Power Units:", censusCompany. },
                    { "MCS-150 Form Date:", censusCompany.MCS_150_DATE?.ToString("d") },
                    //{ "Out of Service Date:", censusCompany. },
                    //{ "State Carrier ID Number:", censusCompany. },
                    //{ "DUNS Number:", censusCompany. },
                    { "Drivers:", censusCompany.TOT_DRS?.ToString() },
                    { "MCS-150 Mileage (Year):", censusCompany.MCS150MILEAGEYEAR?.ToString() },
                };
                if (censusCompany.McNumber != null) {
                    datCompany.Fields["MC/MX/FF Number(s):"] = $"MC-{censusCompany.McNumber}";
                }
                datCompany.Name = censusCompany.NAME;
                datCompany.McNumber = censusCompany.McNumber;
                datCompany.DotNumber = censusCompany.DOT_NUMBER;
                if (String.IsNullOrEmpty(censusCompany.TEL_NUM) == false) {
                    datCompany.Contacts.Add(PhoneContactConverter.ToVm(censusCompany.TEL_NUM));
                }
                if (String.IsNullOrEmpty(censusCompany.EMAILADDRESS) == false) {
                    datCompany.Contacts.AddEmail(censusCompany.EMAILADDRESS);
                }
                if (datCompany.Fields.ContainsKey("Entity Type:") == true) {
                    if (EnumExtensions.TryParseByDesctiption<DatCompanyType>(datCompany.Fields["Entity Type:"], out var type) == true) {
                        datCompany.EntityType = type;
                    }
                }
                // <---------- SET FIELDS AND PROPERTIES FROM CENSUS

                BaseDispatchDialog dialog;
                switch (Model.CompanyType) {
                    case DatCompanyType.Broker:
                        dialog = new EditBrokerDialog(_dispatchService, datCompany, Model.ActualCompany,
                            DispatchBoardViewModel.Instance.ActualCompanies, censusCompany.EMAILADDRESS);
                        break;
                    case DatCompanyType.Carrier:
                        dialog = new EditCarrierDialog(_dispatchService, datCompany, Model.ActualCompany,
                             DispatchBoardViewModel.Instance.ActualCompanies, censusCompany.EMAILADDRESS);
                        break;
                    default:
                        throw new Exception();
                }
                if (dialog.ShowDialog() == true) {
                    Filter_TextChanged(null, null);
                }
            }
        }

        private void gridCompanies_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var censusCompany = (e.OriginalSource as FrameworkElement)?.DataContext as CensusCompanyViewModel;
            if (censusCompany != null) {
                gridCompanies.SelectedItem = censusCompany;
            }
        }

        private void CarrierRegistration_Click(object sender, RoutedEventArgs e) {
            var dotNumber = ((CensusCompanyViewModel)((FrameworkContentElement)e.OriginalSource).DataContext).DOT_NUMBER;
            Process.Start(new ProcessStartInfo($"https://safer.fmcsa.dot.gov/query.asp?searchtype=ANY&query_type=queryCarrierSnapshot&query_param=USDOT&query_string={dotNumber}") { UseShellExecute = true });
        }

        private void CarrierOverview_Click(object sender, RoutedEventArgs e) {
            var dotNumber = ((CensusCompanyViewModel)((FrameworkContentElement)e.OriginalSource).DataContext).DOT_NUMBER;
            Process.Start(new ProcessStartInfo($"https://ai.fmcsa.dot.gov/SMS/Carrier/{dotNumber}/Overview.aspx?FirstView=True") { UseShellExecute = true });
        }

        private void CompanySnapshot_Click(object sender, RoutedEventArgs e) {
            var dotNumber = ((CensusCompanyViewModel)((FrameworkContentElement)e.OriginalSource).DataContext).DOT_NUMBER;
            Process.Start(new ProcessStartInfo($"https://ai.fmcsa.dot.gov/SMS/Carrier/{dotNumber}/CarrierRegistration.aspx") { UseShellExecute = true });
        }
    }
}
