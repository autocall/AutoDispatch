using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class SetupPacketInsuranceAgencyViewModel : ViewModel {
        public Guid EntityId { get; set; }

        private string number;
        private string email;
        private string fax;
        private string phone;
        private string companyName;
        private bool hasInsurance;

        public bool HasInsurance {
            get => hasInsurance; set {
                if (hasInsurance != value) {
                    hasInsurance = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CompanyName {
            get => companyName; set {
                if (companyName != value) {
                    companyName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Phone {
            get => phone; set {
                if (phone != value) {
                    phone = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Fax {
            get => fax; set {
                if (fax != value) {
                    fax = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Email {
            get => email; set {
                if (email != value) {
                    email = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Number {
            get => number; set {
                if (number != value) {
                    number = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
