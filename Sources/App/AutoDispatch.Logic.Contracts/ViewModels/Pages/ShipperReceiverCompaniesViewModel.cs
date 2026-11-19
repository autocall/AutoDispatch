using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class ShipperReceiverCompaniesViewModel : ViewModel {
        private List<ShipperReceiverCompanyViewModel> _shipperReceiverCompanies = new List<ShipperReceiverCompanyViewModel>();
        private string _searchText;
        private ShipperReceiverCompanyViewModel _selectedShipperReceiverCompany;

        public List<ShipperReceiverCompanyViewModel> ShipperReceiverCompanies {
            get { return _shipperReceiverCompanies; }
            set {
                if (_shipperReceiverCompanies != value) {
                    _shipperReceiverCompanies = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ShipperReceiverCompanyViewModel SelectedShipperReceiverCompany {
            get { return _selectedShipperReceiverCompany; }
            set {
                if (_selectedShipperReceiverCompany != value) {
                    _selectedShipperReceiverCompany = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
