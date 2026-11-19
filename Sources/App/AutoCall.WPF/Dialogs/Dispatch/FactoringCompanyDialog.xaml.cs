using AutoCall.Logic.Settings;
using AutoDispatch.Logic.Converters;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Linq;
using Telerik.Windows.Controls;
using System.Threading.Tasks;
using AutoDispatch.Logic.Services;
using AutoMapper;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoCall.Common.Extensions;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class FactoringCompanyDialog : BaseDispatchDialog {
        public class FactoringCompanyDialogViewModel : ViewModel {
            private ActualCompanyViewModel selectedActualCompany;

            public List<ActualCompanyViewModel> ActualCompanies { get; set; }
            public ActualCompanyViewModel SelectedActualCompany {
                get => selectedActualCompany; set {
                    selectedActualCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public FactoringCompanyDialogViewModel Model {
            get { return DataContext as FactoringCompanyDialogViewModel; }
            set { DataContext = value; }
        }

        private readonly DispatchService Service;
        private readonly IMapper Mapper;

        protected override RadProgressBar ProgressBar => this.progressBar;
        protected override TextBlock ErrorTextBlock => this.tbError;
        protected override Button OkButton => this.btnOk;
        protected override Button CloseButton => this.btnCancel;

        public FactoringCompanyDialog(DispatchService service, IMapper mapper) {
            InitializeComponent();
            this.Service = service;
            this.Mapper = mapper;
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            this.Save(Model.SelectedActualCompany, true);
        }

        private void Save(ActualCompanyViewModel vm, bool close) {
            if (vm.FactoringCompany.IsChanged) {
                if (vm.FactoringCompany.Validate(out var error) == false) {
                    this.ErrorTextBlock.Text = error;
                }
                var dto = ActualCompanyConverter.ToDto(vm);
                this.Service.SaveFactoringCompany(dto.FactoringCompanyJson, dto.EntityId, async (w) => {
                    if (w.Status == ResponseStatus.Success) {
                        await Dispatcher.InvokeAsync(async () => {
                            var newDto = JsonExtension.DeserializeObject<FactoringCompanyDto>(w.Data);
                            var newViewModel = FactoringCompanyConverter.ToVm(newDto);
                            newViewModel.IsChanged = false;
                            var actualCompany =
                                DispatchBoardViewModel.Instance.ActualCompanies.First(x => x.EntityId == vm.EntityId);
                            // сохраняем в текущую ViewModel и DispatchBoardViewModel
                            vm.FactoringCompany = actualCompany.FactoringCompany = newViewModel;
                        });
                    }
                    await SaveCompleted(w, close);
                });
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void ActualCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.RemovedItems.Cast<ActualCompanyViewModel>().Any() == false) {
                return;
            }
            var actualCompany = e.RemovedItems.Cast<ActualCompanyViewModel>().First();
            this.Save(actualCompany, false);
        }
    }
}
