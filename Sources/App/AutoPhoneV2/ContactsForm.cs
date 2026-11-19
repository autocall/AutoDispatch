using AutoCall.Common.Extensions;
using AutoCall.Logic.ViewModels;
using MlSampleWindowCS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace AutoPhoneV2 {
    public partial class ContactsForm : Form {

        public List<ContactItem> _contacts = new List<ContactItem>();
        public ContactItem SelectedContact { get; set; }

        private readonly AutoPhoneV2CallService _callService;

        public ContactsForm(AutoPhoneV2CallService callService) {
            _callService = callService;
            InitializeComponent();

            this.TopMost = true;

            this.LoadData();
        }

        public class ContactItem : ICloneable {
            public string Number { get; set; }
            public string Header { get; set; }

            public object Clone() {
                return new ContactItem() {
                    Header = this.Header,
                    Number = this.Number
                };
            }

            public void Asset(ContactItem dataContext) {
                this.Header = dataContext.Header;
                this.Number = dataContext.Number;
            }
        }

        private void LoadData() {
            SettingsCallServiceViewModel callSettings = _callService.GetCallServiceSettings();
            if (callSettings.MicroSIPSettings != null && callSettings.MicroSIPSettings.Contacts != null) {
                foreach (var contact in callSettings.MicroSIPSettings.Contacts) {
                    dataGridView1.Rows.Add(new string[] { contact.Name, contact.Number.ToPhone() });
                    _contacts.Add(new ContactItem() { Header = contact.Name, Number = contact.Number });
                }
            }
        }

        private void SaveData() {
            SettingsCallServiceViewModel callSettings = _callService.GetCallServiceSettings();
            if (callSettings.MicroSIPSettings == null) {
                callSettings.MicroSIPSettings = new MicroSIPSettingsViewModel();
            }
            if (callSettings.MicroSIPSettings.Contacts == null) {
                callSettings.MicroSIPSettings.Contacts = new ObservableCollection<MicroSIPContactViewModel>();
            } else {
                callSettings.MicroSIPSettings.Contacts.Clear();
            }
            foreach (DataGridViewRow row in dataGridView1.Rows) {
                callSettings.MicroSIPSettings.Contacts.Add(
                    new MicroSIPContactViewModel() { Name = (string)row.Cells[0].Value, Number = (string)row.Cells[1].Value });
            }
            // V1: mainManager.SetCallServiceSettings + SaveSettings; MainManager.SetCallServiceSettings теперь сохраняет сам
            _callService.SetCallServiceSettings(callSettings);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e) {
            dataGridView1.ClearSelection();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.ColumnIndex <= 1) {
                Close();
                if (e.RowIndex != -1 && e.RowIndex < _contacts.Count) {
                    SelectedContact = _contacts[e.RowIndex];
                }
            } else if (e.ColumnIndex == 2) {
                var form = new AddContactForm((ContactItem)_contacts[e.RowIndex].Clone());
                if (form.ShowDialog() == DialogResult.OK) {
                    _contacts[e.RowIndex].Asset(form.DataContext);
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = form.DataContext.Header;
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = form.DataContext.Number;
                    // edit
                    this.SaveData();
                }
            } else if (e.ColumnIndex == 3) {
                if (MessageBox.Show(
                    "Are you sure you want to delete selected Company?",
                    "Delete Contact",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) == DialogResult.OK) {
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                    // delete
                    this.SaveData();
                }
            }
        }

        private void AddContactButton_Click(object sender, EventArgs e) {
            var form = new AddContactForm();
            if (form.ShowDialog() == DialogResult.OK) {
                dataGridView1.Rows.Add(new string[] { form.DataContext.Header, form.DataContext.Number });
                //add;
                this.SaveData();
            }
        }
    }
}
