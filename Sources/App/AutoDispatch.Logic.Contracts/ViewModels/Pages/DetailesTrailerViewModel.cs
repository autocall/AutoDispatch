using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCall.Common.Collections;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class DetailesTrailerViewModel : ViewModel {
        public DriverViewModel Driver { get; set; }
        public TruckViewModel Truck { get; set; }
        public TrailerViewModel Trailer { get; set; }
        public ObservableCollection<Wrapper1LoadViewModel> Loads { get; set; }
        public IList<LoadViewModel> AllLoads { get; set; }

        public ActualCompanyViewModel ActualCompany { get; }

        public DetailesTrailerViewModel(
                DriverViewModel driver,
                TruckViewModel truck,
                TrailerViewModel trailer,
                IList<LoadViewModel> loads,
                ActualCompanyViewModel actualCompany) {
            this.Driver = driver;
            this.Truck = truck;
            this.Trailer = trailer;
            this.AllLoads = loads;
            this.ActualCompany = actualCompany;

            Loads = new ObservableCollection<Wrapper1LoadViewModel>();

            Load();
        }

        #region [ Public ]

        public void Reload() {
            Loads.Clear();
            Load();
        }

        private void Load() {
            Loads.AddRange(
                AllLoads.Where(l =>
                    l.BoardType == DispatchBoardType.Carrier &&
                    l.ContainerType == ContainerType.None && // or by ParentId
                    l.TrailerId == Trailer.EntityId)
                .Select(l => new Wrapper1LoadViewModel(l) {
                    ActualCompany = this.ActualCompany,
                    Driver = this.Driver,
                    Truck = this.Truck,
                    Trailer = this.Trailer,
                })
                .OrderBy(l => l.Load.IndexString));

            this.Trailer.GeneratePallets(this.AllLoads);
            OnPropertyChanged("Loads");
            CalcIndexes();
        }

        private void CalcIndexes() {
            for (int i = 0; i < this.Loads.Count; i++) {
                var wrapper = this.Loads[i];
                wrapper.Load.RelativeIndex = (byte)(i + 1);
                for (int j = 0; j < wrapper.Children.Count; j++) {
                    wrapper.Children[j].Load.RelativeIndex = (byte)(j + 1);

                }
            }
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
