using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AutoCall.Common.Interfaces;
using AutoDispatch.Logic.ViewModels;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts {
    public class ContactsViewModel : ViewModel, IAssignable, IComparable {
        public ObservableCollection<ContactBaseViewModel> Contacts { get; private set; }
        public ContactBaseViewModel CurrentContact => Contacts.FirstOrDefault();
        public bool IsEmpty => Contacts.Any() == false;

        public ContactsViewModel() {
            this.Contacts = new ObservableCollection<ContactBaseViewModel>();
            this.Contacts.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(CurrentContact));
        }

        public ContactsViewModel(List<ContactBaseViewModel> list) {
            this.Contacts = new ObservableCollection<ContactBaseViewModel>(list);
            this.Contacts.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(CurrentContact));
        }

        public ContactBaseViewModel GetByType(ContactType type) {
            return this.Contacts.FirstOrDefault(c => c.Type == type);
        }

        public override string ToString() {
            return this.Contacts.FirstOrDefault()?.ToString() ?? String.Empty;
        }

        public void Next() {
            if (Contacts.Any()) {
                var fist = Contacts.First();
                Contacts.Remove(fist);
                Contacts.Add(fist);
            }
        }
        public void Add(ContactBaseViewModel contact) {
            Contacts.Add(contact);
        }

        public void InsertToTop(ContactBaseViewModel contact) {
            Contacts.Insert(0, contact);
        }

        public object Clone() {
            var list = new List<ContactBaseViewModel>();
            foreach (var contact in this.Contacts) {
                list.Add((ContactBaseViewModel)contact.Clone());
            }
            var result = new ContactsViewModel(list);
            return result;
        }

        public void Assign(object source) {
            this.Contacts.Clear();
            foreach (var contact in ((ContactsViewModel)source).Contacts) {
                this.Contacts.Add((ContactBaseViewModel)contact.Clone());
            }
        }

        public int CompareTo(object obj) {
            if (obj is ContactsViewModel) {
                var vm = (ContactsViewModel)obj;
                if (this.Contacts.Count == vm.Contacts.Count) {
                    for (var i = 0; i < this.Contacts.Count; i++) {
                        if (this.Contacts[i].CompareTo(vm.Contacts[i]) != 0) {
                            return -1;
                        }
                    }
                    return 0;
                }
            }
            return -1;
        }

        public void Clear() {
            Contacts.Clear();
        }

        /// <summary>
        ///     Adds new contacts or updates exists </summary>
        public bool Merge(ContactsViewModel contacts) {
            var result = false;
            foreach (ContactBaseViewModel baseContact in contacts.Contacts) {
                switch (baseContact.Type) {
                    case ContactType.Phone: {
                            var newContact = (ContactPhoneViewModel)baseContact;
                            var existsContact = Contacts.FirstOrDefault(c =>
                                c.Type == newContact.Type &&
                                ((ContactPhoneViewModel)c).Phone == newContact.Phone &&
                                ((ContactPhoneViewModel)c).Ext == newContact.Ext) as ContactPhoneViewModel;
                            // If names are different
                            if (existsContact != null) {
                                if (newContact.Contact != existsContact.Contact) {
                                    // Update contact name
                                    existsContact.Contact = newContact.Contact;
                                    result = true;
                                }
                            } else {
                                var clone = (ContactBaseViewModel)newContact.Clone();
                                Contacts.Insert(0, clone);
                                result = true;
                            }
                            break;
                        }
                    case ContactType.Fax: {
                            var newContact = (ContactFaxViewModel)baseContact;
                            var existsContact = Contacts.FirstOrDefault(c => c.Type == newContact.Type && 
                                ((ContactFaxViewModel)c).Phone == newContact.Phone) as ContactFaxViewModel;
                            // If numbers are different
                            if (existsContact == null) {
                                var clone = (ContactBaseViewModel)newContact.Clone();
                                Contacts.Insert(0, clone);
                                result = true;
                            }
                            break;
                        }
                    case ContactType.Email: {
                            var newContact = (ContactEmailViewModel)baseContact;
                            var existsContact = Contacts.FirstOrDefault(c => c.Type == newContact.Type && 
                                ((ContactEmailViewModel)c).Email == newContact.Email) as ContactEmailViewModel;
                            // If emails are different
                            if (existsContact == null) {
                                var clone = (ContactBaseViewModel)newContact.Clone();
                                Contacts.Insert(0, clone);
                                result = true;
                            }
                            break;
                        }
                }
            }
            return result;
        }

        /// <summary>
        ///     Checks exists and add new email </summary>
        public void AddEmail(string email) {
            if (Contacts.Any(e => e is ContactEmailViewModel && ((ContactEmailViewModel)e).Email == email) == false) {
                Contacts.Add(new ContactEmailViewModel() { Email = email });
            }
        }
    }
}