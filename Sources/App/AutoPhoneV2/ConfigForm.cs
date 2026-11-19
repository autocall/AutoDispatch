using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SIPVoipSDK;

namespace AutoPhoneV2 {
    public partial class ConfigForm : Form {
        public CConfig phoneCfg;
        public string m_defLoadStorePath;
        public string m_versionStr;
        public string m_addrStr;

        public ConfigForm() {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e) {
            //Fill play devices list
            int sdCount = phoneCfg.PlaybackDeviceCount;
            for (int i = 0; i < sdCount; i++) comboBoxPlayback.Items.Add(phoneCfg.PlaybackDevice[i]);

            //Fill record devices list
            int rdCount = phoneCfg.RecordDeviceCount;
            for (int i = 0; i < rdCount; i++) comboBoxRecord.Items.Add(phoneCfg.get_RecordDevice(i));

            //Fill network interfaces
            int ntCount = phoneCfg.NetworkInterfaceCount;
            for (int i = 0; i < ntCount; i++) comboBoxNetwork.Items.Add(phoneCfg.get_NetworkInterface(i));

            //Fill video devices
            int vdCount = phoneCfg.VideoDeviceCount;
            for (int i = 0; i < vdCount; i++) comboBoxVideo.Items.Add(phoneCfg.get_VideoDevice(i));

            //Fill STUN items
            comboBoxSTUN.Items.Add("stun.l.google.com:19302");
            comboBoxSTUN.Items.Add("stun.voxgratia.org");
            comboBoxSTUN.Items.Add("stun.voxalot.com");
            comboBoxSTUN.Items.Add("stun.sipgate.net");
            comboBoxSTUN.Items.Add("");

            //Display version and IP
            //textBoxVersion.Text = m_versionStr.Replace("ABTO SIP", "");
            textBoxIP.Text = m_addrStr;

            //Display config data in controls
            SetDataToControls();
        }

        private void SetDataToControls() {
            comboBoxPlayback.SelectedIndex = comboBoxPlayback.Items.IndexOf(phoneCfg.ActivePlaybackDevice);
            comboBoxRecord.SelectedIndex = comboBoxRecord.Items.IndexOf(phoneCfg.ActiveRecordDevice);
            comboBoxNetwork.SelectedIndex = comboBoxNetwork.Items.IndexOf(phoneCfg.ActiveNetworkInterface);
            comboBoxVideo.SelectedIndex = comboBoxVideo.Items.IndexOf(phoneCfg.ActiveVideoDevice);

            comboBoxSTUN.Text = phoneCfg.StunServer;
            sipPortBox.Text = phoneCfg.ListenPort.ToString();

            echoCheckBox.Checked = (phoneCfg.EchoCancelationEnabled == 0) ? false : true;
            noiseCheckBox.Checked = (phoneCfg.NoiseReductionEnabled == 0) ? false : true;
            autogainCheckBox.Checked = (phoneCfg.AutoGainControlEnabled == 0) ? false : true;

            receiveVolupdCheckBox.Checked = (phoneCfg.VolumeUpdateSubscribed == 0) ? false : true;
            playDialToneCheckBox.Checked = (phoneCfg.DialToneEnabled == 0) ? false : true;
            recordMP3CheckBox.Checked = (phoneCfg.MP3RecordingEnabled == 0) ? false : true;
            encryptedCallCheckBox.Checked = (phoneCfg.EncryptedCallEnabled == 0) ? false : true;
            videoCallCheckBox.Checked = (phoneCfg.VideoCallEnabled == 0) ? false : true;
            autoAnswerCheckBox.Checked = (phoneCfg.AutoAnswerEnabled == 0) ? false : true;

            //textBoxUserAgent.Text   = phoneCfg.UserAgent;
            //textBoxCallerId.Text    = phoneCfg.CallerId;

            //sipDomainTextBox.Text   = phoneCfg.RegDomain;
            //sipUserTextBox.Text     = phoneCfg.RegUser;
            //sipPasswordTextBox.Text = phoneCfg.RegPass;
            //authIdTextBox.Text      = phoneCfg.RegAuthId;
            //expireTimeTextBox.Text  = phoneCfg.RegExpire.ToString();

            //sipProxyDomainTextBox.Text  = phoneCfg.ProxyDomain;
            //sipProxyUserTextBox.Text    = phoneCfg.ProxyUser;
            //sipProxyPasswordTextBox.Text= phoneCfg.ProxyPass;


            //Codecs
            int codecCount = phoneCfg.CodecCount;
            checkedListBoxCodec.Items.Clear();
            for (int i = 0; i < codecCount; ++i) {
                checkedListBoxCodec.Items.Add(phoneCfg.get_CodecName(i), (phoneCfg.get_CodecSelected(i) == 0) ? false : true);
            }
        }

        private void GetDataFromControls() {
            //Set new values
            phoneCfg.StunServer = comboBoxSTUN.Text;
            phoneCfg.ListenPort = (sipPortBox.Text == "") ? 0 : int.Parse(sipPortBox.Text);

            phoneCfg.ActivePlaybackDevice = (comboBoxPlayback.SelectedItem != null) ? comboBoxPlayback.SelectedItem.ToString() : "";
            phoneCfg.ActiveRecordDevice = (comboBoxRecord.SelectedItem != null) ? comboBoxRecord.SelectedItem.ToString() : "";
            phoneCfg.ActiveNetworkInterface = (comboBoxNetwork.SelectedItem != null) ? comboBoxNetwork.SelectedItem.ToString() : "";
            phoneCfg.ActiveVideoDevice = (comboBoxVideo.SelectedItem != null) ? comboBoxVideo.SelectedItem.ToString() : "";

            phoneCfg.EchoCancelationEnabled = (echoCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.NoiseReductionEnabled = (noiseCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.AutoGainControlEnabled = (autogainCheckBox.Checked == true) ? 1 : 0;

            phoneCfg.VolumeUpdateSubscribed = (receiveVolupdCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.DialToneEnabled = (playDialToneCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.MP3RecordingEnabled = (recordMP3CheckBox.Checked == true) ? 1 : 0;
            phoneCfg.EncryptedCallEnabled = (encryptedCallCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.VideoCallEnabled = (videoCallCheckBox.Checked == true) ? 1 : 0;
            phoneCfg.AutoAnswerEnabled = (autoAnswerCheckBox.Checked == true) ? 1 : 0;

            //phoneCfg.UserAgent = textBoxUserAgent.Text;
            //phoneCfg.CallerId  = textBoxCallerId.Text;

            //phoneCfg.RegDomain = sipDomainTextBox.Text;
            //phoneCfg.RegUser   = sipUserTextBox.Text;
            //phoneCfg.RegPass   = sipPasswordTextBox.Text;
            //phoneCfg.RegAuthId = authIdTextBox.Text;
            //phoneCfg.RegExpire = (expireTimeTextBox.Text == "") ? 0 : int.Parse(expireTimeTextBox.Text);

            //phoneCfg.ProxyDomain = sipProxyDomainTextBox.Text;
            //phoneCfg.ProxyUser   = sipProxyUserTextBox.Text;
            //phoneCfg.ProxyPass   = sipProxyPasswordTextBox.Text;

            phoneCfg.SetCodecOrder(GetSelectedCodecsAsStr(), 0);
        }

        private void buttonUp_Click(object sender, EventArgs e) {
            int selectedIndex = checkedListBoxCodec.SelectedIndex;

            if (selectedIndex >= 1) {
                MoveCodec(selectedIndex, selectedIndex - 1);
            }
        }

        private void buttonDown_Click(object sender, EventArgs e) {
            int selectedIndex = checkedListBoxCodec.SelectedIndex;

            if (selectedIndex != -1 && selectedIndex < checkedListBoxCodec.Items.Count - 1) {
                MoveCodec(selectedIndex, selectedIndex + 1);
            }
        }

        private void MoveCodec(int fromIdx, int toIdx) {
            string codecNameLabel = checkedListBoxCodec.Items[fromIdx].ToString();
            bool check = checkedListBoxCodec.GetItemChecked(fromIdx);
            checkedListBoxCodec.Items.RemoveAt(fromIdx);

            checkedListBoxCodec.Items.Insert(toIdx, codecNameLabel);
            checkedListBoxCodec.SetItemChecked(toIdx, check);
            checkedListBoxCodec.SelectedIndex = toIdx;
        }

        private string GetSelectedCodecsAsStr() {
            StringBuilder sb = new StringBuilder();
            int count = checkedListBoxCodec.Items.Count;
            for (int i = 0; i < count; ++i) {
                if (checkedListBoxCodec.GetItemChecked(i) == false) continue;

                string codec = checkedListBoxCodec.Items[i].ToString();
                sb.Append(codec);
                sb.Append("|");
            }

            return sb.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            GetDataFromControls();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }//buttonOK_Click

        private void buttonCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonLoadFromIni_Click(object sender, EventArgs e) {
            //Select file
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.InitialDirectory = m_defLoadStorePath;
            fileDlg.Filter = "Config Files (*.ini)|*.ini|All Files (*.*)|*.*||";

            if (fileDlg.ShowDialog() != DialogResult.OK) return;

            //Try to load
            long succeded = phoneCfg.Load(fileDlg.FileName);
            if (succeded == 0) {
                MessageBox.Show("Can't load selected file.\nCheck it has right format like '<key>=<value>'");
                return;
            }

            //Displ new values
            SetDataToControls();
        }

        private void buttonSaveToIni_Click(object sender, EventArgs e) {
            //Select file
            SaveFileDialog fileDlg = new SaveFileDialog();
            fileDlg.InitialDirectory = m_defLoadStorePath;
            fileDlg.Filter = "Config Files (*.ini)|*.ini|All Files (*.*)|*.*||";
            fileDlg.OverwritePrompt = true;

            if (fileDlg.ShowDialog() != DialogResult.OK) return;

            //Store settings
            GetDataFromControls();

            //Store
            phoneCfg.Store(fileDlg.FileName);
        }

    }//ConfigForm
}
