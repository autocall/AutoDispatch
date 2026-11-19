using AutoDispatch.Logic.ViewModels.Items;
using System;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class FactoringCompanyViewModel : ViewModel, ICloneable {
        private bool _hasFactoringCompany;
        private string _error;
        private string companyName;
        private string address;
        private string city;
        private string phone;
        private string email;

        public bool IsChanged { get; set; }

        public bool HasFactoringCompany {
            get => _hasFactoringCompany;
            set {
                _hasFactoringCompany = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }
        }
        public string CompanyName {
            get => companyName; set {
                companyName = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }
        }
        public string Address {
            get => address; set {
                address = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }
        }
        public string City {
            get => city; set {
                city = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }
        }
        public string Phone {
            get => phone; set {
                phone = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }
        }
        public string Email {
            get => email; set {
                email = value;
                this.IsChanged = true;
                OnPropertyChanged();
            }

        }

        public object Clone() {
            var cloned = new FactoringCompanyViewModel {
                HasFactoringCompany = this.HasFactoringCompany,
                CompanyName = this.CompanyName,
                Address = this.Address,
                City = this.City,
                Phone = this.Phone,
                Email = this.Email
            };
            cloned.IsChanged = false;
            return cloned;
        }

        public bool Validate(out string Error) {
            Error = string.Empty;
            if (HasFactoringCompany) {
                if (string.IsNullOrEmpty(this.CompanyName)) {
                    Error = "Please enter a company name";
                    return false;
                }
                if (string.IsNullOrEmpty(this.Address)) {
                    Error = "Please enter an address";
                    return false;
                }
                if (string.IsNullOrEmpty(this.City)) {
                    Error = "Please enter a city";
                    return false;
                }
                return true;
            } else {
                return true;
            }
        }
    }
}
