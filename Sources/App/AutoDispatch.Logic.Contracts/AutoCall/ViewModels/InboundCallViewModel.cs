using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace AutoCall.Logic.ViewModels {
    public class InboundCallViewModel : BaseViewModel {
        private string _phone;
        private string _displayName;
        private string _carrierName;
        private int _rating;
        private Guid? _carrierId;
        private Visibility _visibility = Visibility.Collapsed;

        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); ; OnPropertyChanged(nameof(PhoneFormat)); } }
        public string PhoneFormat => ToPhoneNumber(Phone);
        public string DisplayName { get => _displayName; set { _displayName = value; OnPropertyChanged(); } }
        public string CarrierName { get => _carrierName; set { _carrierName = value; OnPropertyChanged(); } }
        public int Rating { get => _rating; set { _rating = value; OnPropertyChanged(); OnPropertyChanged(nameof(RatingVisibility)); } }
        public Visibility RatingVisibility => Rating != 0 ? Visibility.Visible : Visibility.Collapsed;
        public Guid? CarrierId { get => _carrierId; set { _carrierId = value; OnPropertyChanged(); } }
        public Visibility Visibility { get => _visibility; set { _visibility = value; OnPropertyChanged(); } }

//#if DEBUG
//        public InboundCallViewModel() {
//            Phone = "7083008563";
//            DisplayName = "bestamerican1wico";
//            Visibility = Visibility.Visible;
//            CarrierName = "CARACOL TRUKING";
//        }
//#endif

        protected string ToPhoneNumber(string phone) {
            if (String.IsNullOrEmpty(phone)) {
                return null;
            }
            if (phone.Length < 10) {
                return phone;
            }
            var chars = phone.Where(Char.IsDigit).ToArray();
            return new StringBuilder(14)
                .Append('(')
                .Append(chars, 0, 3)
                .Append(") ")
                .Append(chars, 3, 3)
                .Append('-')
                .Append(chars, 6, 4)
                .ToString();
        }
    }
}
