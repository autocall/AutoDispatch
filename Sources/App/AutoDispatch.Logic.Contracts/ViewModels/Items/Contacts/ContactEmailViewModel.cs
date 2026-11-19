using System;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public class ContactEmailViewModel : ContactBaseViewModel, IComparable {
        private string _email;

        public string Email {
            get { return _email; }
            set {
                if (_email != value) {
                    Match match = Regex.Match(value, ".+<(?<email>.+@.+[.].+)>");
                    if (match.Length > 0) {
                        _email = match.Groups["email"].Value;
                    } else {
                        _email = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public override ContactType Type => ContactType.Email;

        public override int CompareTo(object obj) {
            if (obj is ContactEmailViewModel) {
                var vm = (ContactEmailViewModel)obj;
                if (base.CompareTo(obj) == 0 && this.Email == vm.Email) {
                    return 0;
                }
            }
            return -1;
        }

        public override object Clone() {
            return new ContactEmailViewModel() {
                Email = Email?.Trim()
            };
        }

        public override void Assign(object source) {
            Email = ((ContactEmailViewModel) source).Email;
        }

        public override string ToString() {
            return Email;
        }
    }
}