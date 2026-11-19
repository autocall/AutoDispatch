using System;
using AutoCall.Common.Interfaces;
using AutoDispatch.Logic.ViewModels;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public abstract class ContactBaseViewModel : ViewModel, IAssignable, IComparable {
        public abstract ContactType Type { get; }

        public abstract object Clone();

        public abstract void Assign(object source);

        public virtual int CompareTo(object obj) {
            if (obj is ContactBaseViewModel) {
                var vm = (ContactBaseViewModel)obj;
                if (this.Type == vm.Type) {
                    return 0;
                }
            }
            return -1;
        }

        public string ToString2() {
            if (this is ContactEmailViewModel) {
                return ((ContactEmailViewModel)this).ToString();
            }
            if (this is ContactFaxViewModel) {
                return ((ContactFaxViewModel)this).ToString();
            }
            if (this is ContactPhoneViewModel) {
                return ((ContactPhoneViewModel)this).ToString();
            }
            throw new Exception("Undefined contact type: " + this.GetType());
        }
    }
}