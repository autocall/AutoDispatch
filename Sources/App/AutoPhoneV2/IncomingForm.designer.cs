namespace AutoPhoneV2
{
	partial class IncomingForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IncomingForm));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxCaller = new System.Windows.Forms.TextBox();
			this.textBoxLine = new System.Windows.Forms.TextBox();
			this.buttonNo = new System.Windows.Forms.Button();
			this.buttonYes = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Caller:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(5, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Line:";
			// 
			// textBoxCaller
			// 
			this.textBoxCaller.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxCaller.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textBoxCaller.Location = new System.Drawing.Point(47, 12);
			this.textBoxCaller.Name = "textBoxCaller";
			this.textBoxCaller.ReadOnly = true;
			this.textBoxCaller.Size = new System.Drawing.Size(235, 20);
			this.textBoxCaller.TabIndex = 0;
			// 
			// textBoxLine
			// 
			this.textBoxLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textBoxLine.Location = new System.Drawing.Point(47, 37);
			this.textBoxLine.Name = "textBoxLine";
			this.textBoxLine.ReadOnly = true;
			this.textBoxLine.Size = new System.Drawing.Size(235, 20);
			this.textBoxLine.TabIndex = 1;
			// 
			// buttonNo
			// 
			this.buttonNo.Location = new System.Drawing.Point(165, 106);
			this.buttonNo.Name = "buttonNo";
			this.buttonNo.Size = new System.Drawing.Size(75, 23);
			this.buttonNo.TabIndex = 4;
			this.buttonNo.Text = "No";
			this.buttonNo.UseVisualStyleBackColor = true;
			this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
			// 
			// buttonYes
			// 
			this.buttonYes.Location = new System.Drawing.Point(54, 106);
			this.buttonYes.Name = "buttonYes";
			this.buttonYes.Size = new System.Drawing.Size(75, 23);
			this.buttonYes.TabIndex = 3;
			this.buttonYes.Text = "Yes";
			this.buttonYes.UseVisualStyleBackColor = true;
			this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label3.Location = new System.Drawing.Point(51, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Answer?";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(18, 64);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Padding = new System.Windows.Forms.Padding(2, 3, 0, 0);
			this.pictureBox1.Size = new System.Drawing.Size(23, 23);
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// IncomingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(294, 141);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.buttonYes);
			this.Controls.Add(this.textBoxLine);
			this.Controls.Add(this.textBoxCaller);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "IncomingForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "IncomingForm";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonNo;
		private System.Windows.Forms.Button buttonYes;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox textBoxCaller;
		public System.Windows.Forms.TextBox textBoxLine;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}