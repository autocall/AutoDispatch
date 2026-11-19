using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class DatCompaniesViewModel : ViewModel {
        private List<DatCompanyViewModel> _datCompanies = new List<DatCompanyViewModel>();
        private string _searchText;
        private DatCompanyViewModel _selectedDatCompany;

        public List<DatCompanyViewModel> DatCompanies {
            get { return _datCompanies; }
            set {
                if (_datCompanies != value) {
                    _datCompanies = value;
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

        public DatCompanyViewModel SelectedDatCompany {
            get { return _selectedDatCompany; }
            set {
                if (_selectedDatCompany != value) {
                    _selectedDatCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public ActualCompanyViewModel ActualCompany { get; set; }
    }
}
