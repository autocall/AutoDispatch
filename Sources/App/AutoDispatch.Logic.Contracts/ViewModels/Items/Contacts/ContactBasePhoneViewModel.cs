using System;
using System.Text;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public abstract class ContactBasePhoneViewModel : ContactBaseViewModel, IComparable {
        private string _phone;
        private string _ext;

        public string Phone {
            get { return _phone; }
            set {
                if (_phone != value) {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Ext {
            get { return _ext; }
            set {
                if (_ext != value) {
                    _ext = value;
                    OnPropertyChanged();
                }
            }
        }

        public override int CompareTo(object obj) {
            if (obj is ContactBasePhoneViewModel) {
                var vm = (ContactBasePhoneViewModel)obj;
                if (base.CompareTo(obj) == 0 && this.Phone == vm.Phone && this.Ext == vm.Ext) {
                    return 0;
                }
            }
            return -1;
        }

        public override string ToString() {
            if (String.IsNullOrEmpty(Phone)) {
                return "-";
            }

            string phone;

            var result = new StringBuilder(19);

            var count1 = 3;
            var count2 = 3;
            var count3 = 4;
            // +1 (800) 000-0000
            if (Phone.Length > 10) {
                phone = $"+{Phone.Substring(0, Phone.Length - 10)} ({Phone.Substring(Phone.Length - count1 - count2 - count3, count1)}) {Phone.Substring(Phone.Length - count2 - count3, count2)}-{Phone.Substring(Phone.Length - count3, count3)}";
            } else 
            // (800) 000-0000
            if(Phone.Length == 10) {
                phone = $"({Phone.Substring(0, Phone.Length - count2 - count3)}) {Phone.Substring(Phone.Length - count2 - count3, count2)}-{Phone.Substring(Phone.Length - count3, count3)}";
            } else {
                phone = Phone;
            }

            if (String.IsNullOrEmpty(Ext)) {
                return $"{phone}";
            } else {
                return $"{phone} ({Ext})";
            }
        }
    }
}