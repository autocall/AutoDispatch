using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoPhoneV2
{
	public partial class ActivationForm : Form
	{
		public string m_licUserId;
		public string m_licKey;
		
		public ActivationForm()
		{
			InitializeComponent();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			m_licUserId = licUserIdTextBox.Text;
			m_licKey    = licKeyTextBox.Text;
			if((m_licUserId.Length == 0) || (m_licKey.Length == 0)) return;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void webPageLabel_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://voipsipsdk.com") { UseShellExecute = true });
		}
		
	}
}