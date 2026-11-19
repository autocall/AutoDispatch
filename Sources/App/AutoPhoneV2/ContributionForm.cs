using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AutoPhoneV2
{
	public partial class ContributionForm : Form
	{		
		public int m_inputGain = 100;
		public int m_outputGain= 100;

		public ContributionForm()
		{
			InitializeComponent();
		}

		private void ContributionForm_Load(object sender, EventArgs e)
		{
			inputGainBar.Value  = m_inputGain;
			outputGainBar.Value = m_outputGain;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			//Get values
			m_inputGain  = inputGainBar.Value;
			m_outputGain = outputGainBar.Value;
		
			this.DialogResult = DialogResult.OK;
			this.Close();		
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}		
	}
}
