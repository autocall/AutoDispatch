using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class CustomerCompaniesViewModel : ViewModel {
        private List<CustomerCompanyViewModel> _brokerCompanies = new List<CustomerCompanyViewModel>();
        private string _searchText;
        private CustomerCompanyViewModel _selectedCustomerCompany;

        public List<CustomerCompanyViewModel> CustomerCompanies {
            get { return _brokerCompanies; }
            set {
                if (_brokerCompanies != value) {
                    _brokerCompanies = value;
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

        public CustomerCompanyViewModel SelectedCustomerCompany {
            get { return _selectedCustomerCompany; }
            set {
                if (_selectedCustomerCompany != value) {
                    _selectedCustomerCompany = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
