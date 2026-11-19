using System;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public class ContactFaxViewModel : ContactBasePhoneViewModel, IComparable {
        public override ContactType Type => ContactType.Fax;

        public override object Clone() {
            return new ContactFaxViewModel() {
                Phone = Phone,
                Ext = Ext
            };
        }

        public override void Assign(object source) {
            Phone = ((ContactFaxViewModel)source).Phone;
            Ext = ((ContactFaxViewModel)source).Ext;
        }

        public new string ToString() {
            return $"fax: {base.ToString()}";
        }
    }
}