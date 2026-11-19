namespace AutoPhoneV2
{
	partial class ContributionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Label2 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.outputGainBar = new System.Windows.Forms.TrackBar();
			this.inputGainBar = new System.Windows.Forms.TrackBar();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.outputGainBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.inputGainBar)).BeginInit();
			this.SuspendLayout();
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(6, 61);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(61, 13);
			this.Label2.TabIndex = 38;
			this.Label2.Text = "OutputGain";
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(6, 18);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(53, 13);
			this.Label1.TabIndex = 39;
			this.Label1.Text = "InputGain";
			// 
			// outputGainBar
			// 
			this.outputGainBar.Location = new System.Drawing.Point(68, 51);
			this.outputGainBar.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.outputGainBar.Maximum = 100;
			this.outputGainBar.Name = "outputGainBar";
			this.outputGainBar.Size = new System.Drawing.Size(212, 42);
			this.outputGainBar.TabIndex = 37;
			this.outputGainBar.TickFrequency = 10;
			// 
			// inputGainBar
			// 
			this.inputGainBar.Location = new System.Drawing.Point(68, 9);
			this.inputGainBar.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.inputGainBar.Maximum = 100;
			this.inputGainBar.Name = "inputGainBar";
			this.inputGainBar.Size = new System.Drawing.Size(212, 42);
			this.inputGainBar.TabIndex = 36;
			this.inputGainBar.TickFrequency = 10;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(161, 100);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 35;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(50, 100);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 34;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// ContributionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(294, 133);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.outputGainBar);
			this.Controls.Add(this.inputGainBar);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContributionForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Set Connection Contribution";
			this.Load += new System.EventHandler(this.ContributionForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.outputGainBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.inputGainBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label1;
		private System.Windows.Forms.TrackBar outputGainBar;
		private System.Windows.Forms.TrackBar inputGainBar;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
	}
}