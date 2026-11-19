using System;
using System.Windows.Media.Imaging;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class ActualCompanyViewModel : BaseObjectViewModel {
        private BitmapImage _logoSmall;
        private BitmapImage _logoBig;

        public BitmapImage LogoSmall {
            get { return _logoSmall; }
            set {
                _logoSmall = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage LogoBig {
            get { return _logoBig; }
            set {
                _logoBig = value;
                OnPropertyChanged();
            }
        }

        public int McNumber { get; set; }

        public string LegalName { get; set; }

        public string PhysicalAddress { get; set; }
        public string MailingAddress { get; set; }

        public string ShippersPhone { get; set; }
        public string CellPhone { get; set; }

        public string WorkPhone { get; set; }
        public string WorkFax { get; set; }
        public string WorkEmail { get; set; }

        public string SMTP_SchemeEmail { get; set; }

        public string SMS_Phone { get; set; }

        public string ConfirmationFile { get; set; }
        public string BOLFile { get; set; }
        public string InvoiceFile { get; set; }

        public string ConfirmationData { get; set; }
        public string BOLData { get; set; }
        public string InvoiceData { get; set; }

        public string BnsfLogin { get; set; }
        public string BnsfLastMessage { get; set; }

        public string PaymentInstructions { get; set; }

        public string CarrierViewToken { get; set; }

        public DatCompanyType EntityType { get; set; }

        public DispatchBoardType BoardType { get; set; }

        public FactoringCompanyViewModel FactoringCompany { get; set; }

        public override object Clone() {
            var cloned = base.Clone();
            ((ActualCompanyViewModel)cloned).FactoringCompany = (FactoringCompanyViewModel)FactoringCompany.Clone();
            return cloned;
        }

        public override bool Filter(string search) {
            throw new NotImplementedException();
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            throw new NotImplementedException();
        }
    }
}
