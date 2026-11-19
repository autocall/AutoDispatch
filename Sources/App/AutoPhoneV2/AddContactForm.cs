using System;
using System.Windows.Forms;
using static AutoPhoneV2.ContactsForm;

namespace AutoPhoneV2 {
    public partial class AddContactForm : Form {

        public ContactItem DataContext { get; set; }

        public AddContactForm() {
            InitializeComponent();


            if (DataContext != null) {
                titleTextBox.Text = DataContext.Header;
                numberMaskedTextBox.Text = DataContext.Number;
            }
        }
        public AddContactForm(ContactItem dataContext) {
            InitializeComponent();

            titleTextBox.Text = dataContext.Header;
            numberMaskedTextBox.Text = dataContext.Number;
        }

        private void OkButton_Click(object sender, EventArgs e) {
            if (numberMaskedTextBox.MaskFull) {
                DataContext = new ContactItem() { Header = titleTextBox.Text, Number = numberMaskedTextBox.Text };
                DialogResult = DialogResult.OK;
            } else {
                numberMaskedTextBox.Focus();
                DialogResult = DialogResult.None;
            }
        }
    }
}
