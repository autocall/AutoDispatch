using System;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public class ContactPhoneViewModel : ContactBasePhoneViewModel, IComparable {
        private string _contact;

        public string Contact {
            get { return _contact; }
            set {
                if (_contact != value) {
                    _contact = value;
                    OnPropertyChanged();
                }
            }
        }

        public override ContactType Type => ContactType.Phone;

        public override int CompareTo(object obj) {
            if (obj is ContactPhoneViewModel) {
                var vm = (ContactPhoneViewModel)obj;
                if (base.CompareTo(obj) == 0 && this.Contact == vm.Contact) {
                    return 0;
                }
            }
            return -1;
        }

        public override object Clone() {
            return new ContactPhoneViewModel() {
                Phone = Phone,
                Ext = Ext,
                Contact = Contact?.Trim()
            };
        }

        public override void Assign(object source) {
            Phone = ((ContactPhoneViewModel)source).Phone;
            Ext = ((ContactPhoneViewModel)source).Ext;
            Contact = ((ContactPhoneViewModel)source).Contact;
        }

        public new string ToString() {
            return $"phone: {base.ToString()} {Contact}";
        }
    }
}