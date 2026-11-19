using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCall.Common.Collections;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class DetailesCarrierViewModel : ViewModel {

        #region [ _fields ]

        private Wrapper1LoadViewModel _selectedLoad;

        private DatCompanyViewModel _datCompany;

        private IList<DatDriverViewModel> _allDrivers;
        private IList<LoadViewModel> _allLoads;
        private IList<ActualCompanyViewModel> _actualCompanies;
        private ActualCompanyViewModel _actualCompany;

        #endregion [ _fields ]

        #region [ Properties ]

        public DatCompanyViewModel DatCompany { get; private set; }

        public IList<DatDriverViewModel> AllDrivers { get; private set; }
        public IList<LoadViewModel> AllLoads { get; private set; }
        public IList<ActualCompanyViewModel> ActualCompanies { get; private set; }
        public ActualCompanyViewModel ActualCompany { get; private set; }

        public Wrapper1LoadViewModel SelectedLoad {
            get { return _selectedLoad; }
            set {
                if (_selectedLoad != value) {
                    _selectedLoad = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Wrapper1LoadViewModel> SelectedLoads { get; set; } = new ObservableCollection<Wrapper1LoadViewModel>();

        #endregion [ Properties ]

        #region [ .ctor ]

        public DetailesCarrierViewModel(
            DatCompanyViewModel company,
            IList<DatDriverViewModel> drivers,
            IList<LoadViewModel> loads,
            IList<ActualCompanyViewModel> actualCompanies,
            ActualCompanyViewModel actualCompany) {

            this._datCompany = company;
            this._allDrivers = drivers;
            this._allLoads = loads;
            this._actualCompanies = actualCompanies;
            this._actualCompany = actualCompany;

            SelectedLoads.CollectionChanged += (sender, args) => OnPropertyChanged("SelectedLoads");

            this.DatCompany = (DatCompanyViewModel)this._datCompany.Clone();
            this.AllDrivers = this._allDrivers.Select(e => (DatDriverViewModel)e.Clone()).ToList();
            this.AllLoads = this._allLoads.Select(e => (LoadViewModel)e.Clone()).ToList();
            this.ActualCompanies = this._actualCompanies.Select(e => (ActualCompanyViewModel)e.Clone()).ToList();
            this.ActualCompany = (ActualCompanyViewModel)this._actualCompany.Clone();

            Load();
        }

        #endregion [ .ctor ]

        #region [ Public ]

        public void Reload() {
            SelectedLoads.Clear();
            DatCompany.Clear();
            Load();
        }

        private void Load() {
            // Вытаскиваем грузы по DatCompany
            DatCompany.Loads = new ObservableCollection<Wrapper1LoadViewModel>(
                AllLoads.Where(l =>
                    l.BoardType == DispatchBoardType.Broker &&
                    l.DatCompanyId == DatCompany.EntityId &&
                    l.Status != LoadStatus.Completed)
                .OrderBy(l => l.BindDate)
                .Select(l => new Wrapper1LoadViewModel(l) {
                    DatCompany = this.DatCompany,
                    ActualCompany = this.ActualCompany
                }));
            foreach (Wrapper1LoadViewModel wrapper in DatCompany.Loads) {
                if(wrapper.Load.DatDriverId != null) {
                    wrapper.DatDriver = AllDrivers.First(e => e.EntityId == wrapper.Load.DatDriverId);
                }
            }
            foreach (Wrapper1LoadViewModel load in DatCompany.Loads) {
                load.Children.AddRange(
                    AllLoads
                        .Where(l => load.Load.EntityId == l.ShipmentId || load.Load.EntityId == l.ParentId)
                        .Select(l => new Wrapper1LoadViewModel(l) {
                            DatCompany = this.DatCompany,
                            ActualCompany = this.ActualCompany
                        })
                    );
            }
            CalcIndexes();
        }

        private void CalcIndexes() {
            for (byte i = 0; i < this.DatCompany.Loads.Count; i++) {
                this.DatCompany.Loads[i].Load.RelativeIndex = (byte)(i + 1);
                for (byte j = 0; j < this.DatCompany.Loads[i].Children.Count; j++) {
                    this.DatCompany.Loads[i].Children[j].Load.RelativeIndex = (byte)(j + 1);
                }
            }
        }

        public void SetDriver(DatDriverViewModel driver) {
            foreach (var driver1 in AllDrivers) {
                if (driver1.EntityId == driver.EntityId) {
                    driver1.Assign(driver);
                    return;
                }
            }
            AllDrivers.Add((DatDriverViewModel)driver.Clone());
            Reload();
        }

        public async Task SetLoads(List<LoadViewModel> loads) {
            foreach (var load2 in loads) {
                foreach (var load1 in AllLoads) {
                    if (load1.EntityId == load2.EntityId) {
                        if (load2.BoardType == load1.BoardType) {
                            load1.Assign(load2);
                            break;
                        } else {
                            // another board
                            AllLoads.Remove(load1);
                            break;
                        }
                    }
                }
            }
            Reload();
        }

        #endregion [ Public ]
    }
}
