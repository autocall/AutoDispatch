using AutoCall.Common.Enums;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections;
using Telerik.Windows.Data;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class CarrierHistoryViewModel : ViewModel, IPagedCollectionView, IEnumerable {
        private int totalItemCount;
        private int itemCount;
        private int pageIndex;
        private int pageSize = 100;
        private AutoCallEquipmentType? equipment;
        private LoadType? loadType;
        private bool brokerLoading;
        private string brokerName;
        private string createdBy;

        public DispatchBoardViewModel ViewModel { get; set; } = new DispatchBoardViewModel();

        #region [ Filter ]

        public DateTime? MinPickupDate { get; set; }
        public DateTime? MaxPickupDate { get; set; }
        public DateTime? MinDeliveryDate { get; set; }
        public DateTime? MaxDeliveryDate { get; set; }

        public LocationViewModel Origin { get; set; }
        public LocationViewModel Destination { get; set; }
        public int? OriginRadius { get; set; }
        public int? DestinationRadius { get; set; }

        public int? MinPallets { get; set; }
        public int? MaxPallets { get; set; }
        public int? MinWeight { get; set; }
        public int? MaxWeight { get; set; }
        public int? MinLineHaul { get; set; }
        public int? MaxLineHaul { get; set; }

        public string LoadId { get; set; }
        public string ProId { get; set; }

        public AutoCallEquipmentType? Equipment {
            get => equipment;
            set {
                equipment = value;
                OnPropertyChanged();
            }
        }

        public LoadType? LoadType {
            get => loadType;
            set {
                loadType = value;
                OnPropertyChanged();
            }
        }

        public bool BrokerLoading {
            get => brokerLoading;
            set {
                brokerLoading = value;
                OnPropertyChanged();
            }
        }

        public string BrokerName {
            get => brokerName;
            set {
                brokerName = value;
                OnPropertyChanged();
            }
        }

        public string CreatedBy {
            get => createdBy;
            set {
                createdBy = value;
                OnPropertyChanged();
            }
        }

        #endregion [ Filter ]

        #region [ IPagedCollectionView  ]

        public bool CanChangePage { get; } = true;
        public bool IsPageChanging { get; }
        public int ItemCount {
            get => itemCount;
            set {
                itemCount = value;
                OnPropertyChanged();
            }
        }
        public int PageIndex {
            get => pageIndex;
            set {
                pageIndex = value;
                OnPropertyChanged();
            }
        }
        public int PageSize {
            get => pageSize;
            set {
                pageSize = value;
                OnPropertyChanged();
            }
        }
        public int TotalItemCount {
            get => totalItemCount;
            set {
                totalItemCount = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<EventArgs> PageChanged;
        public event EventHandler<PageChangingEventArgs> PageChanging;

        public IEnumerator GetEnumerator() {
            return this.ViewModel.Wrappers.GetEnumerator();
        }

        public bool MoveToFirstPage() {
            if (this.PageIndex > 0) {
                this.PageIndex = 0;
                PageChanged(this, null);
                return true;
            } else {
                return false;
            }
        }
        public bool MoveToLastPage() {
            if (this.PageIndex < this.ItemCount / this.pageSize) {
                this.PageIndex = this.ItemCount / this.pageSize;
                PageChanged(this, null);
                return true;
            } else {
                return false;
            }
        }
        public bool MoveToNextPage() {
            if (this.PageIndex < this.ItemCount / this.pageSize) {
                this.PageIndex++;
                PageChanged(this, null);
                return true;
            } else {
                return false;
            }
        }
        public bool MoveToPage(int pageIndex) {
            if (this.PageIndex > 0 && this.PageIndex < this.ItemCount / this.pageSize) {
                // NOTES: работает без события
                return true;
            } else {
                return false;
            }
        }
        public bool MoveToPreviousPage() {
            if (this.PageIndex > 0) {
                this.PageIndex--;
                PageChanged(this, null);
                return true;
            } else {
                return false;
            }
        }

        #endregion [ IPagedCollectionView ]
    }
}
