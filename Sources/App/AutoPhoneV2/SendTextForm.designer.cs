namespace AutoPhoneV2
{
	partial class SendTextForm
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
			this.textBoxMessage = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxAddress = new System.Windows.Forms.TextBox();
			this.buttonSend = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBoxMessage
			// 
			this.textBoxMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxMessage.Location = new System.Drawing.Point(2, 2);
			this.textBoxMessage.Multiline = true;
			this.textBoxMessage.Name = "textBoxMessage";
			this.textBoxMessage.Size = new System.Drawing.Size(394, 58);
			this.textBoxMessage.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(-1, 67);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "TO (SIP number):";
			// 
			// textBoxAddress
			// 
			this.textBoxAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxAddress.Location = new System.Drawing.Point(94, 63);
			this.textBoxAddress.Name = "textBoxAddress";
			this.textBoxAddress.Size = new System.Drawing.Size(241, 20);
			this.textBoxAddress.TabIndex = 1;
			// 
			// buttonSend
			// 
			this.buttonSend.Location = new System.Drawing.Point(341, 63);
			this.buttonSend.Name = "buttonSend";
			this.buttonSend.Size = new System.Drawing.Size(55, 20);
			this.buttonSend.TabIndex = 3;
			this.buttonSend.Text = "Send";
			this.buttonSend.UseVisualStyleBackColor = true;
			this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
			// 
			// SendTextForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(399, 88);
			this.Controls.Add(this.buttonSend);
			this.Controls.Add(this.textBoxAddress);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxMessage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SendTextForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Send Text Message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxMessage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxAddress;
		private System.Windows.Forms.Button buttonSend;
	}
}