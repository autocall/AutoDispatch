using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AutoPhoneV2
{

	public partial class JoinForm : Form
	{
		public int m_curLineId = 0;
		public int m_selLineId = 0;

		public struct ComboLineItem
		{
			public int    handle;
			public string name;

			public ComboLineItem(int _handle)
			{
				handle = _handle;

				StringBuilder sb = new StringBuilder();
				sb.Append("Line");
				sb.Append(handle);
				name = sb.ToString();
			}

			public override string ToString()			
			{
				return this.name;
			}
		}//ComboLineItem
				
		public JoinForm()
		{
			InitializeComponent();
		}


		private void JoinForm_Load(object sender, EventArgs e)
		{
			for(int i = 1; i <= PhoneLinesHelper.MaxLines; ++i)
			{
				if(m_curLineId == i) continue;
				comboBoxLine.Items.Add( new ComboLineItem(i));
			}
			comboBoxLine.SelectedIndex = 0;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			//Get sel line
			ComboLineItem item = (ComboLineItem)comboBoxLine.Items[comboBoxLine.SelectedIndex];			
			m_selLineId = item.handle;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
				

	}//JoinForm
}