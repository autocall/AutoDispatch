using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AutoPhoneV2
{
	public partial class SendTextForm : Form
	{
		public SendTextForm()
		{
			InitializeComponent();
		}

		public string address
		{
			get { return textBoxAddress.Text;  }
			set { textBoxAddress.Text = value; }
		}

		public string message
		{
			get { return textBoxMessage.Text; }
		}


		private void buttonSend_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}