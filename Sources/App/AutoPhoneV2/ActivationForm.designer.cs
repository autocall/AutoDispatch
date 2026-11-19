namespace AutoPhoneV2
{
	partial class ActivationForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivationForm));
			this.label1 = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.licUserIdTextBox = new System.Windows.Forms.TextBox();
			this.licKeyTextBox = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.webPageLabel = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(100, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(348, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "Note: By default license data will be stored in file \'phoneCfg.ini\', in applicati" +
				"on setup directory";
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(270, 152);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::AutoPhoneV2.Properties.Resources.activate;
			this.pictureBox1.InitialImage = null;
			this.pictureBox1.Location = new System.Drawing.Point(10, 10);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(36, 38);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(100, 44);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(94, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Look for details on";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label14.Location = new System.Drawing.Point(10, 69);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(84, 13);
			this.label14.TabIndex = 4;
			this.label14.Text = "License User Id:";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label15.Location = new System.Drawing.Point(10, 93);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(68, 13);
			this.label15.TabIndex = 6;
			this.label15.Text = "License Key:";
			// 
			// licUserIdTextBox
			// 
			this.licUserIdTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.licUserIdTextBox.Location = new System.Drawing.Point(100, 67);
			this.licUserIdTextBox.Name = "licUserIdTextBox";
			this.licUserIdTextBox.Size = new System.Drawing.Size(348, 20);
			this.licUserIdTextBox.TabIndex = 0;
			// 
			// licKeyTextBox
			// 
			this.licKeyTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.licKeyTextBox.Location = new System.Drawing.Point(100, 91);
			this.licKeyTextBox.Multiline = true;
			this.licKeyTextBox.Name = "licKeyTextBox";
			this.licKeyTextBox.Size = new System.Drawing.Size(348, 52);
			this.licKeyTextBox.TabIndex = 1;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(373, 152);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// webPageLabel
			// 
			this.webPageLabel.AutoSize = true;
			this.webPageLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.webPageLabel.Location = new System.Drawing.Point(195, 44);
			this.webPageLabel.Name = "webPageLabel";
			this.webPageLabel.Size = new System.Drawing.Size(107, 13);
			this.webPageLabel.TabIndex = 27;
			this.webPageLabel.TabStop = true;
			this.webPageLabel.Text = "voipsipsdk.com";
			this.webPageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.webPageLabel.Click += new System.EventHandler(this.webPageLabel_Click);
			// 
			// ActivationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(459, 183);
			this.Controls.Add(this.webPageLabel);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.licUserIdTextBox);
			this.Controls.Add(this.licKeyTextBox);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ActivationForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Activate SIP SDK";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox licUserIdTextBox;
		private System.Windows.Forms.TextBox licKeyTextBox;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.LinkLabel webPageLabel;
	}
}