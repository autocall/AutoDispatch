using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoDispatch.Logic.Contracts.ViewModels {
    public class CensusOfCompaniesViewModel : ViewModel {
        private CensusCompanySearchViewModel _search;
        private List<CensusCompanyViewModel> _companies = new List<CensusCompanyViewModel>();
        private bool _processing;
        private DatCompanyType _companyType;

        public DatCompanyType CompanyType {
            get => _companyType;
            set {
                _companyType = value;
                OnPropertyChanged();
            }
        }

        public CensusCompanySearchViewModel Search {
            get => _search;
            set {
                _search = value;
                OnPropertyChanged();
            }
        }
        public List<CensusCompanyViewModel> Companies {
            get => _companies;
            set {
                _companies = value;
                OnPropertyChanged();
            }
        }

        public bool Processing {
            get => _processing;
            set {
                _processing = value;
                OnPropertyChanged();
            }
        }

        public List<DatCompanyViewModel> DatCompanies { get; set; }
        /// <summary>
        ///     Selected Actual Company </summary>
        public ActualCompanyViewModel ActualCompany { get; set; }

        public void CalcSmallLogo() {
            if (Companies != null && DatCompanies != null) {
                foreach (var company in Companies) {
                    foreach (var datCompany in DatCompanies) {
                        if ((company.McNumber != null && company.McNumber == datCompany.McNumber) ||
                            (company.DOT_NUMBER == datCompany.DotNumber)) {
                            company.LogoSmall = DispatchBoardViewModel.Instance.GetActualCompanyById(datCompany.ActualCompanyId)?.LogoSmall;
                        }
                    }
                }
            }
        }
    }
}
