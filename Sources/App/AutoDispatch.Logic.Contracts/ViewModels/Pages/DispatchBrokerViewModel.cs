using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public partial class DispatchBoardViewModel {
        private ActualCompanyViewModel _selectedBrokerCompany;

        #region [ Public ]

        public ActualCompanyViewModel SelectedBrokerCompany {
            get { return _selectedBrokerCompany; }
            set {
                _selectedBrokerCompany = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewDatCompany { get; private set; }

        private void RefreshIsNewDatCompanies() {
            this.IsNewDatCompany = DatCompanies.Any(e => e.SetupPacketCompleted && !e.SetupPacketAccept);
            OnPropertyChanged(nameof(this.IsNewDatCompany));
        }

        /// <summary>
        ///     Adds or updates companies </summary>
        public async Task SetDatCompaniesAsync(IList<DatCompanyViewModel> viewModels, bool refreshWrappers = true) {
            if (viewModels == null) return;
            Stopwatch sw0 = Stopwatch.StartNew();
            Debug.WriteLine($"SetDatCompanies({viewModels.Count}) -->");
            foreach (var viewModel in viewModels) {
                var oldViewModel = DatCompanies.FirstOrDefault(i => i.EntityId == viewModel.EntityId);
                if (oldViewModel != null) {
                    if (viewModel.IsDeleted == false) {
                        // Replace
                        oldViewModel.Assign(viewModel);
                    } else {
                        // Delete
                        DatCompanies.Remove(oldViewModel);
                    }
                } else {
                    // Add
                    DatCompanies.Add(viewModel);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.DatCompany);
            // Refreshes gif button on the toolbar
            RefreshIsNewDatCompanies();
            RefreshCompanies(viewModels);
            Debug.WriteLine($"SetTrailers({viewModels.Count}) {sw0.Elapsed.TotalSeconds:F2}sec");
        }

        /// <summary>
        ///     Adds or updates drivers </summary>
        public async Task SetDatDriversAsync(IList<DatDriverViewModel> viewModels, bool refreshWrappers = true) {
            if (viewModels == null) return;
            foreach (var viewModel in viewModels) {
                var oldViewModel = DatDrivers.FirstOrDefault(i => i.EntityId == viewModel.EntityId);
                if (oldViewModel != null) {
                    if (viewModel.IsDeleted == false) {
                        // Replace
                        oldViewModel.Assign(viewModel);
                    } else {
                        // Delete
                        DatDrivers.Remove(oldViewModel);
                    }
                } else {
                    // Add
                    DatDrivers.Add(viewModel);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.DatDriver);
        }

        /// <summary>
        ///     Adds or updates SetupPackets </summary>
        public async Task SetSetupPacketsAsync(IList<SetupPacketViewModel> viewModels, bool refreshWrappers = true) {
            if (viewModels == null) return;
            Stopwatch sw0 = Stopwatch.StartNew();
            Debug.WriteLine($"SetSetupPackets({viewModels.Count}) -->");
            foreach (var viewModel in viewModels) {
                var oldViewModel = SetupPackets.FirstOrDefault(i => i.EntityId == viewModel.EntityId);
                if (oldViewModel != null) {
                    if (viewModel.IsDeleted == false) {
                        // Replace
                        oldViewModel.Assign(viewModel);
                    } else {
                        // Delete
                        SetupPackets.Remove(oldViewModel);
                    }
                } else {
                    // Add
                    SetupPackets.Add(viewModel);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.SetupPacket);
            Debug.WriteLine($"SetSetupPackets({viewModels.Count}) {sw0.Elapsed.TotalSeconds:F2}sec");
        }

        #endregion [ Public ]
    }
}
