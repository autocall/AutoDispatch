namespace AutoPhoneV2
{
	partial class ConfigForm
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
            this.sipPortBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxRecord = new System.Windows.Forms.ComboBox();
            this.comboBoxPlayback = new System.Windows.Forms.ComboBox();
            this.autogainCheckBox = new System.Windows.Forms.CheckBox();
            this.echoCheckBox = new System.Windows.Forms.CheckBox();
            this.noiseCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
            this.groupBoxCodec = new System.Windows.Forms.GroupBox();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.checkedListBoxCodec = new System.Windows.Forms.CheckedListBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxNetwork = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxVideo = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBoxOther = new System.Windows.Forms.GroupBox();
            this.comboBoxSTUN = new System.Windows.Forms.ComboBox();
            this.encryptedCallCheckBox = new System.Windows.Forms.CheckBox();
            this.videoCallCheckBox = new System.Windows.Forms.CheckBox();
            this.autoAnswerCheckBox = new System.Windows.Forms.CheckBox();
            this.recordMP3CheckBox = new System.Windows.Forms.CheckBox();
            this.playDialToneCheckBox = new System.Windows.Forms.CheckBox();
            this.receiveVolupdCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBoxManage = new System.Windows.Forms.GroupBox();
            this.buttonSaveToIni = new System.Windows.Forms.Button();
            this.buttonLoadFromIni = new System.Windows.Forms.Button();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.groupBoxSound.SuspendLayout();
            this.groupBoxCodec.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxOther.SuspendLayout();
            this.groupBoxManage.SuspendLayout();
            this.SuspendLayout();
            // 
            // sipPortBox
            // 
            this.sipPortBox.Location = new System.Drawing.Point(91, 17);
            this.sipPortBox.Name = "sipPortBox";
            this.sipPortBox.Size = new System.Drawing.Size(106, 20);
            this.sipPortBox.TabIndex = 1;
            this.sipPortBox.Text = "5060";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(36, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Microphone:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(52, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Speaker:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(6, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "SIP Listen Port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "STUN:";
            // 
            // comboBoxRecord
            // 
            this.comboBoxRecord.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRecord.FormattingEnabled = true;
            this.comboBoxRecord.Location = new System.Drawing.Point(108, 49);
            this.comboBoxRecord.Name = "comboBoxRecord";
            this.comboBoxRecord.Size = new System.Drawing.Size(320, 21);
            this.comboBoxRecord.TabIndex = 3;
            // 
            // comboBoxPlayback
            // 
            this.comboBoxPlayback.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPlayback.FormattingEnabled = true;
            this.comboBoxPlayback.Location = new System.Drawing.Point(108, 22);
            this.comboBoxPlayback.Name = "comboBoxPlayback";
            this.comboBoxPlayback.Size = new System.Drawing.Size(320, 21);
            this.comboBoxPlayback.TabIndex = 1;
            // 
            // autogainCheckBox
            // 
            this.autogainCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autogainCheckBox.Location = new System.Drawing.Point(4, 63);
            this.autogainCheckBox.Name = "autogainCheckBox";
            this.autogainCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.autogainCheckBox.Size = new System.Drawing.Size(194, 17);
            this.autogainCheckBox.TabIndex = 8;
            this.autogainCheckBox.Text = "Auto gain control";
            this.autogainCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.autogainCheckBox.UseVisualStyleBackColor = true;
            // 
            // echoCheckBox
            // 
            this.echoCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.echoCheckBox.Location = new System.Drawing.Point(4, 17);
            this.echoCheckBox.Name = "echoCheckBox";
            this.echoCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.echoCheckBox.Size = new System.Drawing.Size(194, 17);
            this.echoCheckBox.TabIndex = 6;
            this.echoCheckBox.Text = "Echo cancellation";
            this.echoCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.echoCheckBox.UseVisualStyleBackColor = true;
            // 
            // noiseCheckBox
            // 
            this.noiseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.noiseCheckBox.Location = new System.Drawing.Point(4, 40);
            this.noiseCheckBox.Name = "noiseCheckBox";
            this.noiseCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.noiseCheckBox.Size = new System.Drawing.Size(194, 17);
            this.noiseCheckBox.TabIndex = 7;
            this.noiseCheckBox.Text = "Noise reduction";
            this.noiseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.noiseCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBoxSound
            // 
            this.groupBoxSound.Controls.Add(this.autogainCheckBox);
            this.groupBoxSound.Controls.Add(this.echoCheckBox);
            this.groupBoxSound.Controls.Add(this.noiseCheckBox);
            this.groupBoxSound.Location = new System.Drawing.Point(8, 167);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Size = new System.Drawing.Size(204, 90);
            this.groupBoxSound.TabIndex = 10;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "Sound";
            // 
            // groupBoxCodec
            // 
            this.groupBoxCodec.Controls.Add(this.buttonDown);
            this.groupBoxCodec.Controls.Add(this.buttonUp);
            this.groupBoxCodec.Controls.Add(this.checkedListBoxCodec);
            this.groupBoxCodec.Location = new System.Drawing.Point(219, 260);
            this.groupBoxCodec.Name = "groupBoxCodec";
            this.groupBoxCodec.Size = new System.Drawing.Size(230, 190);
            this.groupBoxCodec.TabIndex = 12;
            this.groupBoxCodec.TabStop = false;
            this.groupBoxCodec.Text = "Codec";
            // 
            // buttonDown
            // 
            this.buttonDown.Location = new System.Drawing.Point(147, 13);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(75, 20);
            this.buttonDown.TabIndex = 1;
            this.buttonDown.Text = "MoveDown";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonUp
            // 
            this.buttonUp.Location = new System.Drawing.Point(7, 13);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(75, 20);
            this.buttonUp.TabIndex = 0;
            this.buttonUp.Text = "MoveUp";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // checkedListBoxCodec
            // 
            this.checkedListBoxCodec.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBoxCodec.FormattingEnabled = true;
            this.checkedListBoxCodec.Location = new System.Drawing.Point(7, 35);
            this.checkedListBoxCodec.Name = "checkedListBoxCodec";
            this.checkedListBoxCodec.Size = new System.Drawing.Size(214, 152);
            this.checkedListBoxCodec.TabIndex = 2;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(293, 459);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(374, 459);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(8, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Network interface:";
            // 
            // comboBoxNetwork
            // 
            this.comboBoxNetwork.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNetwork.FormattingEnabled = true;
            this.comboBoxNetwork.Location = new System.Drawing.Point(108, 76);
            this.comboBoxNetwork.Name = "comboBoxNetwork";
            this.comboBoxNetwork.Size = new System.Drawing.Size(320, 21);
            this.comboBoxNetwork.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxVideo);
            this.groupBox1.Controls.Add(this.comboBoxNetwork);
            this.groupBox1.Controls.Add(this.comboBoxPlayback);
            this.groupBox1.Controls.Add(this.comboBoxRecord);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(437, 140);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Devices";
            // 
            // comboBoxVideo
            // 
            this.comboBoxVideo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideo.FormattingEnabled = true;
            this.comboBoxVideo.Location = new System.Drawing.Point(108, 103);
            this.comboBoxVideo.Name = "comboBoxVideo";
            this.comboBoxVideo.Size = new System.Drawing.Size(320, 21);
            this.comboBoxVideo.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(28, 105);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "Video Device:";
            // 
            // groupBoxOther
            // 
            this.groupBoxOther.Controls.Add(this.comboBoxSTUN);
            this.groupBoxOther.Controls.Add(this.encryptedCallCheckBox);
            this.groupBoxOther.Controls.Add(this.videoCallCheckBox);
            this.groupBoxOther.Controls.Add(this.autoAnswerCheckBox);
            this.groupBoxOther.Controls.Add(this.recordMP3CheckBox);
            this.groupBoxOther.Controls.Add(this.playDialToneCheckBox);
            this.groupBoxOther.Controls.Add(this.receiveVolupdCheckBox);
            this.groupBoxOther.Controls.Add(this.label2);
            this.groupBoxOther.Controls.Add(this.label7);
            this.groupBoxOther.Controls.Add(this.sipPortBox);
            this.groupBoxOther.Location = new System.Drawing.Point(8, 260);
            this.groupBoxOther.Name = "groupBoxOther";
            this.groupBoxOther.Size = new System.Drawing.Size(204, 190);
            this.groupBoxOther.TabIndex = 16;
            this.groupBoxOther.TabStop = false;
            this.groupBoxOther.Text = "Other";
            // 
            // comboBoxSTUN
            // 
            this.comboBoxSTUN.FormattingEnabled = true;
            this.comboBoxSTUN.Items.AddRange(new object[] {
            "",
            "stun.l.google.com:19302"});
            this.comboBoxSTUN.Location = new System.Drawing.Point(91, 38);
            this.comboBoxSTUN.Name = "comboBoxSTUN";
            this.comboBoxSTUN.Size = new System.Drawing.Size(106, 21);
            this.comboBoxSTUN.TabIndex = 5;
            // 
            // encryptedCallCheckBox
            // 
            this.encryptedCallCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.encryptedCallCheckBox.Location = new System.Drawing.Point(4, 143);
            this.encryptedCallCheckBox.Name = "encryptedCallCheckBox";
            this.encryptedCallCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.encryptedCallCheckBox.Size = new System.Drawing.Size(194, 17);
            this.encryptedCallCheckBox.TabIndex = 8;
            this.encryptedCallCheckBox.Text = "Enable encrypted call";
            this.encryptedCallCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.encryptedCallCheckBox.UseVisualStyleBackColor = true;
            // 
            // videoCallCheckBox
            // 
            this.videoCallCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.videoCallCheckBox.Location = new System.Drawing.Point(4, 162);
            this.videoCallCheckBox.Name = "videoCallCheckBox";
            this.videoCallCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.videoCallCheckBox.Size = new System.Drawing.Size(194, 17);
            this.videoCallCheckBox.TabIndex = 8;
            this.videoCallCheckBox.Text = "Enable video call";
            this.videoCallCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.videoCallCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoAnswerCheckBox
            // 
            this.autoAnswerCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoAnswerCheckBox.Location = new System.Drawing.Point(4, 103);
            this.autoAnswerCheckBox.Name = "autoAnswerCheckBox";
            this.autoAnswerCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.autoAnswerCheckBox.Size = new System.Drawing.Size(194, 17);
            this.autoAnswerCheckBox.TabIndex = 8;
            this.autoAnswerCheckBox.Text = "Auto answer";
            this.autoAnswerCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.autoAnswerCheckBox.UseVisualStyleBackColor = true;
            // 
            // recordMP3CheckBox
            // 
            this.recordMP3CheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.recordMP3CheckBox.Location = new System.Drawing.Point(4, 123);
            this.recordMP3CheckBox.Name = "recordMP3CheckBox";
            this.recordMP3CheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.recordMP3CheckBox.Size = new System.Drawing.Size(194, 17);
            this.recordMP3CheckBox.TabIndex = 8;
            this.recordMP3CheckBox.Text = "Record in MP3";
            this.recordMP3CheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.recordMP3CheckBox.UseVisualStyleBackColor = true;
            // 
            // playDialToneCheckBox
            // 
            this.playDialToneCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.playDialToneCheckBox.Location = new System.Drawing.Point(4, 83);
            this.playDialToneCheckBox.Name = "playDialToneCheckBox";
            this.playDialToneCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.playDialToneCheckBox.Size = new System.Drawing.Size(194, 17);
            this.playDialToneCheckBox.TabIndex = 8;
            this.playDialToneCheckBox.Text = "Play dialtone when calling";
            this.playDialToneCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.playDialToneCheckBox.UseVisualStyleBackColor = true;
            // 
            // receiveVolupdCheckBox
            // 
            this.receiveVolupdCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.receiveVolupdCheckBox.Location = new System.Drawing.Point(4, 63);
            this.receiveVolupdCheckBox.Name = "receiveVolupdCheckBox";
            this.receiveVolupdCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.receiveVolupdCheckBox.Size = new System.Drawing.Size(194, 17);
            this.receiveVolupdCheckBox.TabIndex = 8;
            this.receiveVolupdCheckBox.Text = "Receive OnVolumeUpdate events";
            this.receiveVolupdCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.receiveVolupdCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBoxManage
            // 
            this.groupBoxManage.Controls.Add(this.buttonSaveToIni);
            this.groupBoxManage.Controls.Add(this.buttonLoadFromIni);
            this.groupBoxManage.Location = new System.Drawing.Point(218, 167);
            this.groupBoxManage.Name = "groupBoxManage";
            this.groupBoxManage.Size = new System.Drawing.Size(231, 90);
            this.groupBoxManage.TabIndex = 17;
            this.groupBoxManage.TabStop = false;
            this.groupBoxManage.Text = "Manage config files";
            // 
            // buttonSaveToIni
            // 
            this.buttonSaveToIni.Location = new System.Drawing.Point(7, 51);
            this.buttonSaveToIni.Name = "buttonSaveToIni";
            this.buttonSaveToIni.Size = new System.Drawing.Size(215, 20);
            this.buttonSaveToIni.TabIndex = 0;
            this.buttonSaveToIni.Text = "Save current settings in new file...";
            this.buttonSaveToIni.UseVisualStyleBackColor = true;
            this.buttonSaveToIni.Click += new System.EventHandler(this.buttonSaveToIni_Click);
            // 
            // buttonLoadFromIni
            // 
            this.buttonLoadFromIni.Location = new System.Drawing.Point(7, 19);
            this.buttonLoadFromIni.Name = "buttonLoadFromIni";
            this.buttonLoadFromIni.Size = new System.Drawing.Size(215, 20);
            this.buttonLoadFromIni.TabIndex = 0;
            this.buttonLoadFromIni.Text = "Load from other ini file...";
            this.buttonLoadFromIni.UseVisualStyleBackColor = true;
            this.buttonLoadFromIni.Click += new System.EventHandler(this.buttonLoadFromIni_Click);
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(8, 461);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.ReadOnly = true;
            this.textBoxIP.Size = new System.Drawing.Size(204, 20);
            this.textBoxIP.TabIndex = 18;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(457, 490);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.groupBoxManage);
            this.Controls.Add(this.groupBoxOther);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxCodec);
            this.Controls.Add(this.groupBoxSound);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Application Settings";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.groupBoxSound.ResumeLayout(false);
            this.groupBoxCodec.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxOther.ResumeLayout(false);
            this.groupBoxOther.PerformLayout();
            this.groupBoxManage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox sipPortBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxRecord;
		private System.Windows.Forms.ComboBox comboBoxPlayback;
		private System.Windows.Forms.CheckBox autogainCheckBox;
		private System.Windows.Forms.CheckBox echoCheckBox;
		private System.Windows.Forms.CheckBox noiseCheckBox;
		private System.Windows.Forms.GroupBox groupBoxSound;
		private System.Windows.Forms.GroupBox groupBoxCodec;
		private System.Windows.Forms.Button buttonDown;
		private System.Windows.Forms.Button buttonUp;
		private System.Windows.Forms.CheckedListBox checkedListBoxCodec;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxNetwork;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBoxOther;
		private System.Windows.Forms.CheckBox playDialToneCheckBox;
		private System.Windows.Forms.CheckBox receiveVolupdCheckBox;
		private System.Windows.Forms.CheckBox autoAnswerCheckBox;
		private System.Windows.Forms.CheckBox recordMP3CheckBox;
		private System.Windows.Forms.GroupBox groupBoxManage;
		private System.Windows.Forms.Button buttonSaveToIni;
		private System.Windows.Forms.Button buttonLoadFromIni;
		private System.Windows.Forms.ComboBox comboBoxVideo;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox encryptedCallCheckBox;
		private System.Windows.Forms.CheckBox videoCallCheckBox;
		private System.Windows.Forms.TextBox textBoxIP;
		private System.Windows.Forms.ComboBox comboBoxSTUN;
	}
}