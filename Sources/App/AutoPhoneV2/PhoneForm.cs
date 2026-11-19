using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using SIPVoipSDK;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using MlSampleWindowCS.Services;
using AutoCall.Logic.Services;
using AutoCall.Common.Helpers;
using AutoCall.Common.Extensions;
using Color = System.Drawing.Color;
using AutoCall.Common;
using System.Threading;
using System.Reflection;
using System.Text;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace AutoPhoneV2 {

    public partial class SIPPhoneForm : Form {

        #region [ _fields ]

        public void Debug() {
            //AbtoPhone.Shutdown();
            var dtmf = "1";
            playDtmf(ref dtmf);
        }

        private CAbtoPhone AbtoPhone;
        private AutoPhoneV2CallService CallService;

        private string cfgFileName = "phoneCfg.ini";

        private static List<Tuple<string, string, DateTime, object>> HistoryCalls = new List<Tuple<string, string, DateTime, object>>();

        private bool m_MP3RecordingEnabled = false;
        private bool m_AutoAnswerEnabled = false;

        private string RegisterName = null;

        #endregion [ _fields ]

        #region [ external ]

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        #endregion [ external ]

        #region [ .ctor ]

        // https://stackoverflow.com/questions/1567017/com-object-that-has-been-separated-from-its-underlying-rcw-cannot-be-used
        public _IAbtoPhoneEvents_OnInitializedEventHandler OnInitialized { get; }
        public _IAbtoPhoneEvents_OnLineSwichedEventHandler OnLineSwiched { get; }
        public _IAbtoPhoneEvents_OnEstablishedCallEventHandler OnEstablishedCall { get; }
        public _IAbtoPhoneEvents_OnIncomingCall2EventHandler OnIncomingCall2 { get; }
        public _IAbtoPhoneEvents_OnClearedCallEventHandler OnClearedCall { get; }
        public _IAbtoPhoneEvents_OnRegisteredEventHandler OnRegistered { get; }
        public _IAbtoPhoneEvents_OnPlayFinished2EventHandler OnPlayFinished2 { get; }
        public _IAbtoPhoneEvents_OnEstablishedConnectionEventHandler OnEstablishedConnection { get; }
        public _IAbtoPhoneEvents_OnClearedConnectionEventHandler OnClearedConnection { get; }
        public _IAbtoPhoneEvents_OnTextMessageReceivedEventHandler OnTextMessageReceived { get; }
        public _IAbtoPhoneEvents_OnTextMessageSentStatusEventHandler OnTextMessageSentStatus { get; }
        public _IAbtoPhoneEvents_OnPhoneNotifyEventHandler OnPhoneNotify { get; }
        public _IAbtoPhoneEvents_OnRemoteAlerting2EventHandler OnRemoteAlerting2 { get; }
        public _IAbtoPhoneEvents_OnSubscribeStatusEventHandler OnSubscribeStatus { get; }
        public _IAbtoPhoneEvents_OnRecordFinishedEventHandler OnRecordFinished { get; }
        public _IAbtoPhoneEvents_OnRecordFinished2EventHandler OnRecordFinished2 { get; }
        public _IAbtoPhoneEvents_OnHoldCallEventHandler OnHoldCall { get; }
        public _IAbtoPhoneEvents_OnHoldCall2EventHandler OnHoldCall2 { get; }
        public _IAbtoPhoneEvents_OnReceivedRequestUpdateEventHandler OnReceivedRequestUpdate { get; }

        public SIPPhoneForm() {
            InitializeComponent();

            PhoneLinesHelper.UpdatePanel = UpdatePanel;
            PhoneLinesHelper.UpdateShortcuts = UpdateShortcuts;

            SetWindowPos(this.Handle, HWND_TOPMOST, this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height, TOPMOST_FLAGS);

            this.OnInitialized = new _IAbtoPhoneEvents_OnInitializedEventHandler(this.AbtoPhone_OnInitialized);
            this.OnLineSwiched = new _IAbtoPhoneEvents_OnLineSwichedEventHandler(this.AbtoPhone_OnLineSwiched);
            this.OnEstablishedCall = new _IAbtoPhoneEvents_OnEstablishedCallEventHandler(this.AbtoPhone_OnEstablishedCall);
            this.OnIncomingCall2 = new _IAbtoPhoneEvents_OnIncomingCall2EventHandler(AbtoPhone_OnIncomingCall2);
            this.OnClearedCall = new _IAbtoPhoneEvents_OnClearedCallEventHandler(this.AbtoPhone_OnClearedCall);
            this.OnRegistered = new _IAbtoPhoneEvents_OnRegisteredEventHandler(this.AbtoPhone_OnRegistered);
            this.OnPlayFinished2 = new _IAbtoPhoneEvents_OnPlayFinished2EventHandler(AbtoPhone_OnPlayFinished2);
            this.OnEstablishedConnection = new _IAbtoPhoneEvents_OnEstablishedConnectionEventHandler(this.AbtoPhone_OnEstablishedConnection);
            this.OnClearedConnection = new _IAbtoPhoneEvents_OnClearedConnectionEventHandler(this.AbtoPhone_OnClearedConnection);
            this.OnTextMessageReceived = new _IAbtoPhoneEvents_OnTextMessageReceivedEventHandler(this.AbtoPhone_OnTextMessageReceived);
            this.OnTextMessageSentStatus = new _IAbtoPhoneEvents_OnTextMessageSentStatusEventHandler(AbtoPhone_OnTextMessageSentStatus);
            this.OnPhoneNotify = new _IAbtoPhoneEvents_OnPhoneNotifyEventHandler(this.AbtoPhone_OnPhoneNotify);
            this.OnRemoteAlerting2 = new _IAbtoPhoneEvents_OnRemoteAlerting2EventHandler(AbtoPhone_OnRemoteAlerting2);
            this.OnSubscribeStatus = new _IAbtoPhoneEvents_OnSubscribeStatusEventHandler(AbtoPhone_OnSubscribeStatus);
            this.OnRecordFinished = new _IAbtoPhoneEvents_OnRecordFinishedEventHandler(AbtoPhone_OnRecordFinished);
            this.OnRecordFinished2 = new _IAbtoPhoneEvents_OnRecordFinished2EventHandler(AbtoPhone_OnRecordFinished2);
            this.OnHoldCall = new _IAbtoPhoneEvents_OnHoldCallEventHandler(AbtoPhone_OnHoldCall);
            //this.AbtoPhone.OnReceivedRequestInfo = new _IAbtoPhoneEvents_OnReceivedRequestInfoEventHandler(AbtoPhone_OnReceivedRequestInfo);
            //this.AbtoPhone.OnToneDetected = new _IAbtoPhoneEvents_OnToneDetectedEventHandler(AbtoPhone_OnToneDetected);
            this.OnHoldCall2 = new _IAbtoPhoneEvents_OnHoldCall2EventHandler(AbtoPhone_OnHoldCall2);
            this.OnReceivedRequestUpdate = new _IAbtoPhoneEvents_OnReceivedRequestUpdateEventHandler(AbtoPhone_OnReceivedRequestUpdate);
            //this.AbtoPhone.OnSubscriptionRequest = new _IAbtoPhoneEvents_OnSubscriptionRequestEventHandler(AbtoPhone_OnSubscriptionRequest);
            //this.AbtoPhone.OnSubscriptionNotify = new _IAbtoPhoneEvents_OnSubscriptionNotifyEventHandler(AbtoPhone_OnSubscriptionNotify);
        }

        #endregion [ .ctor ]

        #region [ Get Control by Line ]

        public Panel LinePanel(int lineNumber) => (Panel)linesLayoutPanel.Controls.Find("linePanel" + (lineNumber + 1), false).First();
        public Label LineNumberLabel(int lineNumber) => (Label)LinePanel(lineNumber).Controls.Find("lineNumberLabel" + (lineNumber + 1), false).First();
        public Label Headerlabel(int lineNumber) => (Label)LinePanel(lineNumber).Controls.Find("lineHeaderlabel" + (lineNumber + 1), false).First();
        public Label PhoneLabel(int lineNumber) => (Label)LinePanel(lineNumber).Controls.Find("phoneLabel" + (lineNumber + 1), false).First();
        public Label CallTimelabel(int lineNumber) => (Label)LinePanel(lineNumber).Controls.Find("callTimeLabel" + (lineNumber + 1), false).First();
        public Button HoldButton(int lineNumber) => (Button)LinePanel(lineNumber).Controls.Find("holdButton" + (lineNumber + 1), false).First();
        public Button HungButton(int lineNumber) => (Button)LinePanel(lineNumber).Controls.Find("hungButton" + (lineNumber + 1), false).First();
        public Button ActionsButton(int lineNumber) => (Button)LinePanel(lineNumber).Controls.Find("actionsButton" + (lineNumber + 1), false).First();

        #endregion [ Get Control by Line ]

        #region [ Set Control by Line ]

        public void SetHeader(int lineNumber, string header) {
            Headerlabel(lineNumber).Text = header;
        }

        public void SetLoadId(int lineNumber, string loadId) {
            LinePanel(lineNumber).Tag = loadId;
        }

        public void SetPhone(int lineNumber, string phone) {
            PhoneLabel(lineNumber).Text = phone?.ToPhone();
        }

        public void SetSubHeader(int lineNumber, string subHeader) {
            //SubHeaderlabel.Text = subHeader;
        }

        #endregion [ Set Control by Line ]

        #region [ Update Panels ]

        private void UpdateShortcuts() {
            UpdateShortcuts(PhoneLinesHelper.GetShortcutAutoTransferState(), PhoneLinesHelper.GetShortcutCallState());
        }

        private void UpdateShortcuts(bool transfer, bool call) {
            SetEnabledShortcutAutoTransfer(transfer);
            SetEnabledShortcutCall(call);
        }

        private void UpdatePanel(int number, bool calling, bool established, bool held) {
            if (calling || established) {
                LinePanel(number).Visible = true;
            } else {
                LinePanel(number).Visible = false;
            }
            if (established) {
                // Established
                HungButton(number).BackColor = Color.Red;
                HungButton(number).BackgroundImage = Properties.Resources.HungUp;
                CallTimelabel(number).Visible = true;
            } else {
                CallTimelabel(number).Visible = false;
                if (calling) {
                    // Calling
                    HungButton(number).BackColor = Color.Coral;
                    HungButton(number).BackgroundImage = Properties.Resources.HungUp;
                } else {
                    // Normal
                    HungButton(number).BackgroundImage = Properties.Resources.StartCall;
                    HungButton(number).BackColor = SystemColors.Control;
                }
            }
            if (held) {
                HoldButton(number).BackgroundImage = Properties.Resources.Retrieve;
            } else {
                if (calling) {
                    HoldButton(number).BackgroundImage = Properties.Resources.Hold_gray;
                } else {
                    HoldButton(number).BackgroundImage = Properties.Resources.Hold;
                }
            }
            if (held == false && calling == false && established == true) {
                LineNumberLabel(number).BackColor = Color.Black;
                Headerlabel(number).ForeColor = Color.Black;
                PhoneLabel(number).ForeColor = Color.Black;
                CallTimelabel(number).ForeColor = Color.Black;
                ActionsButton(number).BackgroundImage = Properties.Resources.dots;
            } else {
                LineNumberLabel(number).BackColor = Color.DarkGray;
                Headerlabel(number).ForeColor = Color.DarkGray;
                PhoneLabel(number).ForeColor = Color.DarkGray;
                CallTimelabel(number).ForeColor = Color.DarkGray;
                ActionsButton(number).BackgroundImage = Properties.Resources.dots_gray;
            }
        }

        private void SetEnabledShortcutAutoTransfer(bool value) {
            foreach (var button in shortcutPanel.Controls.Cast<Button>()) {
                var jsonObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject((string)button.Tag);
                var type = Convert.ToInt32(((JValue)jsonObj["Type"]).Value);
                if (type == TRANSFER_SHORTCUT_TYPE) {
                    button.Enabled = value;
                }
            }
        }

        private void SetEnabledShortcutCall(bool value) {
            foreach (var button in shortcutPanel.Controls.Cast<Button>()) {
                var jsonObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject((string)button.Tag);
                var type = Convert.ToInt32(((JValue)jsonObj["Type"]).Value);
                if (type == CALL_SHORTCUT_TYPE) {
                    button.Enabled = value;
                }
            }
        }

        #endregion [ Update Panels ]

        #region [ Load / Close Events ]

        private void SIPPhone_Load(object sender, EventArgs e) {
            //Displ initial status
            displayNotifyMsg("Initializing...");

            //Configure playback controls
            spkVolume.SetRange(0, 100);
            spkVolume.Value = AbtoPhone.PlaybackVolume;
            spkVolume.TickFrequency = 10;

            //Mute Speaker/Microphone
            muteSpeakerFlag.Checked = true;
            muteSpeakerFlag.Image = Properties.Resources.muteout;

            //Get state
            m_MP3RecordingEnabled = AbtoPhone.Config.MP3RecordingEnabled != 0;
            m_AutoAnswerEnabled = AbtoPhone.Config.AutoAnswerEnabled != 0;

            this.toolStripStatusLabel1.Image = Properties.Resources.unknown;

            UpdatePanel(0, false, false, false);
            UpdatePanel(1, false, false, false);
            UpdatePanel(2, false, false, false);
            UpdatePanel(3, false, false, false);

            LoadMedia();

            CallService.Notification(abtophone_state.SET_ACCOUNT, null, null, null);

            notificationsListBox.DrawMode = DrawMode.OwnerDrawVariable;
            notificationsListBox.MeasureItem += lst_MeasureItem;
            notificationsListBox.DrawItem += lst_DrawItem;

            var cm = new ContextMenu();
            cm.MenuItems.Clear();
            cm.MenuItems.Add("Copy", (_sender, _e) => Clipboard.SetText(String.Join("\n", notificationsListBox.Items.Cast<string>())));
            notificationsListBox.ContextMenu = cm;
            notificationsListBox.ReadOnly = true;

            numberComboBox.GotFocus += NumberComboBox_GotFocus;

            // Restores Call Log
            try {
                if (String.IsNullOrEmpty(Properties.Settings.Default.CallLog) == false) {
                    HistoryCalls = JsonExtension.DeserializeObject<List<Tuple<string, string, DateTime, object>>>(Properties.Settings.Default.CallLog);
                    HistoryCalls = HistoryCalls.Where(c => c.Item3.Date == DateTime.Now.Date).ToList();
                }
            } catch (Exception exc) {
            }
        }

        private void lst_MeasureItem(object sender, MeasureItemEventArgs e) {
            e.ItemHeight = (int)e.Graphics.MeasureString(notificationsListBox.Items[e.Index].ToString(), notificationsListBox.Font, notificationsListBox.Width).Height;
        }

        private void lst_DrawItem(object sender, DrawItemEventArgs e) {
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString(notificationsListBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }

        private void SIPPhone_Closing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing && !Object.Equals(this.Tag, "Close")) {
                e.Cancel = true;
            }
        }

        #region [ Most to Top ]

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams {
            get {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        #endregion [ Most to Top ]

        public void TryClose() {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => TryClose()));
                return;
            }
            if (AbtoPhone != null) {
                this.HANGUP_ALL_Action(null);
                SIPPhoneForm_FormClosed(null, null);
                Close();
            }
        }

        #region [ Volume Indicator ]

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        MMDevice device = null;
        private int lastValue = 0;

        private void Timer_Tick(object sender, EventArgs e) {
            if (Thread.CurrentThread.IsAlive == false) return;
            try {
                if (device == null) {
                    device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                }
                var newValue = (int)(100 * device.AudioMeterInformation.MasterPeakValue);
                if (lastValue != newValue) {
                    progressBar1.Value = newValue;
                    lastValue = newValue;
                }
            } catch (Exception exp) {
                displayNotifyMsg($"Err: {exp.Message}");
                timer.Stop();
            }
        }

        #endregion [ Volume Indicator ]

        private void SIPPhoneForm_FormClosed(object sender, FormClosedEventArgs e) {
            if (this.AbtoPhone == null) return;

            timer.Stop();

            this.AbtoPhone.OnInitialized -= this.AbtoPhone_OnInitialized;
            this.AbtoPhone.OnLineSwiched -= this.AbtoPhone_OnLineSwiched;
            this.AbtoPhone.OnEstablishedCall -= this.AbtoPhone_OnEstablishedCall;
            this.AbtoPhone.OnIncomingCall2 -= this.AbtoPhone_OnIncomingCall2;
            this.AbtoPhone.OnClearedCall -= this.AbtoPhone_OnClearedCall;
            this.AbtoPhone.OnRegistered -= this.AbtoPhone_OnRegistered;
            this.AbtoPhone.OnPlayFinished2 -= this.AbtoPhone_OnPlayFinished2;
            this.AbtoPhone.OnEstablishedConnection -= this.AbtoPhone_OnEstablishedConnection;
            this.AbtoPhone.OnClearedConnection -= this.AbtoPhone_OnClearedConnection;
            this.AbtoPhone.OnTextMessageReceived -= this.AbtoPhone_OnTextMessageReceived;
            this.AbtoPhone.OnTextMessageSentStatus -= this.AbtoPhone_OnTextMessageSentStatus;
            this.AbtoPhone.OnPhoneNotify -= this.AbtoPhone_OnPhoneNotify;
            this.AbtoPhone.OnRemoteAlerting2 -= this.AbtoPhone_OnRemoteAlerting2;
            this.AbtoPhone.OnSubscribeStatus -= this.AbtoPhone_OnSubscribeStatus;
            this.AbtoPhone.OnRecordFinished -= this.AbtoPhone_OnRecordFinished;
            this.AbtoPhone.OnRecordFinished2 -= this.AbtoPhone_OnRecordFinished2;
            this.AbtoPhone.OnHoldCall -= this.AbtoPhone_OnHoldCall;
            //this.AbtoPhone.OnReceivedRequestInfo -= this.AbtoPhone_OnReceivedRequestInfo;
            //this.AbtoPhone.OnToneDetected -= this.AbtoPhone_OnToneDetected;
            this.AbtoPhone.OnHoldCall2 -= this.AbtoPhone_OnHoldCall2;
            this.AbtoPhone.OnReceivedRequestUpdate -= this.AbtoPhone_OnReceivedRequestUpdate;
            //this.AbtoPhone.OnSubscriptionRequest -= this.AbtoPhone_OnSubscriptionRequest;
            //this.AbtoPhone.OnSubscriptionNotify -=this.AbtoPhone_OnSubscriptionNotify;

            AbtoPhone.Shutdown();
            //Marshal.ReleaseComObject(this.AbtoPhone); // вроде перестал давать ошибку при завкрытии
            AbtoPhone = null;

            CallService.MAKE_CALL_Action = null;
            CallService.HANGUP_Action = null;
            CallService.HANGUP_ALL_Action = null;
            CallService.TRANSFER_Action = null;
            CallService.ACTION_DTMF_Action = null;
            CallService.HOLD_Action = null;
            CallService.RESUME_Action = null;
            CallService.SET_ACCOUNT_Action = null;
            CallService.DisplayNotifyMsg = null;
            //CallService.Notification = null;

            PhoneLinesHelper.Reset();

            // Saves Call Log
            Properties.Settings.Default.CallLog = JsonExtension.SerializeObject(HistoryCalls);
            Properties.Settings.Default.Save();
        }

        #endregion [ Load / Close Events ]

        public bool ConfigurePhone(AutoPhoneV2CallService callService) {
            // V1: ServiceLocator.Current.GetInstance<IAutoPhoneV2CallService>() ниже по методу
            CallService = callService;
            try {
                timer.Tick += Timer_Tick;
                timer.Interval = 50;
                timer.Start();
            } catch (Exception e) {
                displayNotifyMsg($"Err: {e.Message}");
            }
#if DEBUG
            return false;
#endif
            try {
                //Assign event handlers
                AbtoPhone = new CAbtoPhone();
                this.AbtoPhone.OnInitialized += this.AbtoPhone_OnInitialized;
                this.AbtoPhone.OnLineSwiched += this.AbtoPhone_OnLineSwiched;
                this.AbtoPhone.OnEstablishedCall += this.AbtoPhone_OnEstablishedCall;
                this.AbtoPhone.OnIncomingCall2 += this.AbtoPhone_OnIncomingCall2;
                this.AbtoPhone.OnClearedCall += this.AbtoPhone_OnClearedCall;
                this.AbtoPhone.OnRegistered += this.AbtoPhone_OnRegistered;
                this.AbtoPhone.OnPlayFinished2 += this.AbtoPhone_OnPlayFinished2;
                this.AbtoPhone.OnEstablishedConnection += this.AbtoPhone_OnEstablishedConnection;
                this.AbtoPhone.OnClearedConnection += this.AbtoPhone_OnClearedConnection;
                this.AbtoPhone.OnTextMessageReceived += this.AbtoPhone_OnTextMessageReceived;
                this.AbtoPhone.OnTextMessageSentStatus += this.AbtoPhone_OnTextMessageSentStatus;
                this.AbtoPhone.OnPhoneNotify += this.AbtoPhone_OnPhoneNotify;
                this.AbtoPhone.OnRemoteAlerting2 += this.AbtoPhone_OnRemoteAlerting2;
                this.AbtoPhone.OnSubscribeStatus += this.AbtoPhone_OnSubscribeStatus;
                this.AbtoPhone.OnRecordFinished += this.AbtoPhone_OnRecordFinished;
                this.AbtoPhone.OnRecordFinished2 += this.AbtoPhone_OnRecordFinished2;
                this.AbtoPhone.OnHoldCall += this.AbtoPhone_OnHoldCall;
                //this.AbtoPhone.OnReceivedRequestInfo += this.AbtoPhone_OnReceivedRequestInfo;
                //this.AbtoPhone.OnToneDetected += this.AbtoPhone_OnToneDetected;
                this.AbtoPhone.OnHoldCall2 += this.AbtoPhone_OnHoldCall2;
                this.AbtoPhone.OnReceivedRequestUpdate += this.AbtoPhone_OnReceivedRequestUpdate;
                //this.AbtoPhone.OnSubscriptionRequest += this.AbtoPhone_OnSubscriptionRequest;
                //this.AbtoPhone.OnSubscriptionNotify +=this.AbtoPhone_OnSubscriptionNotify;

                CallService.MAKE_CALL_Action = MAKE_CALL_Action;
                CallService.HANGUP_Action = HANGUP_Action;
                CallService.HANGUP_ALL_Action = HANGUP_ALL_Action;
                CallService.TRANSFER_Action = TRANSFER_Action;
                CallService.ACTION_DTMF_Action = ACTION_DTMF_Action;
                CallService.HOLD_Action = HOLD_Action;
                CallService.RESUME_Action = RESUME_Action;
                CallService.SET_ACCOUNT_Action = SET_ACCOUNT_Action;
                CallService.DisplayNotifyMsg = displayNotifyMsg;

                //Get config
                CConfig phoneCfg = AbtoPhone.Config;

                //Load config values from file
                phoneCfg.Load(cfgFileName);

                //phoneCfg.ExSipAccount_Add("192.168.0.150", "216", "216", "", "", 300, 1, 0);
                //phoneCfg.ExSipAccount_Add("192.168.0.150", "217", "217", "", "", 300, 1, 0);

                //Manual set needed values
                //phoneCfg.ListenPort = 5060;
                //phoneCfg.StunServer = "stun.l.google.com:19302";			

                // обнулим, чтобы избежать повторной авторизации
                phoneCfg.RegDomain = null;
                phoneCfg.RegUser = null;
                phoneCfg.RegPass = null;

                //phoneCfg.RegDomain = "...";//your domain
                //phoneCfg.RegUser = "..";//your user name
                //phoneCfg.RegPass = "..";//your password
                //phoneCfg.RegExpire = 300;
                //phoneCfg.TlsVerifyServerEnabled = false;//Disable server certificate verification (always trust)
                //phoneCfg.SignallingTransport = (int)TransportType.eTransportTLSv1;//Set TLS transport
                //phoneCfg.EncryptedCallEnabled = 1;//Enable SRTP

                phoneCfg.RingToneFile = getSoundFilePath("telephone-ring.wav");

                //Specify License key // 
                phoneCfg.LicenseUserId = "{Licensed_for_Autocall_Windows-BD09-477A-C4983296-6B30-5901-1979-FDA5A4ECD01E}";
                phoneCfg.LicenseKey = "{C8aPhZGJu0Q8LMsV1e2lpdJrQp8msCg47plS5zWerWDY4zGghYkiSpPkISeRuCxtblCHDHfKaMEvB0cNmARUfg==}";
#if DEBUG
                phoneCfg.LogLevel = (LogLevelType)11;//set most detailed log output
                phoneCfg.LogPath = "C:\\temp\\logs";//set own path to load if required
#endif
                //Apply modified config
                AbtoPhone.ApplyConfig();

                AbtoPhone.Initialize();
                AbtoPhone.RecordVolume = 100;

                return true;
            } catch (FileNotFoundException e) {
                this.ExternalExceptionHandler(e);
                return false;
            } catch (COMException e) {
                displayNotifyMsg(e.Message);
                this.ExternalExceptionHandler(e);
                return false;

            }
        }

        private string getSoundFilePath(string fileName) {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\phone\sounds", fileName);
        }

        private void displayNotifyMsg(string msg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => displayNotifyMsg(msg)));
                return;
            }
            notificationsListBox.Items.Add(msg);
            notificationsListBox.TopIndex = notificationsListBox.Items.Count - 1;//scrol down
            if (msg.Contains("ERROR")) {
                _l.e(msg);
            }
        }

        #region [ AutoCall Actions ]

        const int TRANSFER_SHORTCUT_TYPE = 4;
        const int CALL_SHORTCUT_TYPE = 0;

        // json: { "login":"0002", "password":"L;.qvBO2", "callType":0 }
        private void SET_ACCOUNT_Action(string json) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => SET_ACCOUNT_Action(json)));
                return;
            }
            //Get config
            CConfig phoneCfg = AbtoPhone.Config;
            //Load config values from file
            phoneCfg.Load(cfgFileName);

            var jsonObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            phoneCfg.ProxyDomain = phoneCfg.RegDomain = "phone.myautocall.net";
            phoneCfg.ProxyUser = phoneCfg.RegUser = (string)jsonObj["login"];
            phoneCfg.ProxyPass = phoneCfg.RegPass = (string)jsonObj["password"];
            PhoneLinesHelper.MultilineMode = Convert.ToInt32(jsonObj["callType"]) == 1;
            PhoneLinesHelper.MaxLines = PhoneLinesHelper.MultilineMode ? Convert.ToInt32(jsonObj["maxPhoneLines"]) : 1;
            // [{"Label":"CH ROBINSON COMPANY","Number":"8334285247","Type":4},{"Label":"HUB GROUP INC","Number":"8776953986","Type":0}]
            var shortcutsJson = (string)jsonObj["shortcuts"];
            shortcutPanel.Controls.Clear();
            actionsContextMenuStrip.Items.Clear();
            if (String.IsNullOrEmpty(shortcutsJson) == false) {
                actionsContextMenuStrip.Items.Add(new ToolStripMenuItem() { Enabled = false, Text = "Auto Transfer" });
                if (String.IsNullOrEmpty(shortcutsJson) == false) {
                    var shortcuts = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(shortcutsJson);
                    if (shortcuts != null) {
                        for (int i = 0; i < shortcuts.Count; i++) {
                            JToken shortcut = shortcuts[i];
                            var type = Convert.ToInt32(((JValue)shortcut["Type"]).Value);
                            EventHandler eventHandler;
                            bool enabled;
                            if (type == TRANSFER_SHORTCUT_TYPE) {
                                enabled = false;
                                eventHandler = autoTransferShortcutBtn_Click;
                            } else if (type == CALL_SHORTCUT_TYPE) {
                                enabled = true;
                                eventHandler = callShortcutBtn_Click;
                            } else {
                                continue;
                            }
                            var button = new Button() {
                                Text = ((JValue)shortcut["Label"]).Value.ToString(),
                                Dock = DockStyle.Top,
                                Tag = shortcut.ToString(),
                                Enabled = enabled,
                                Margin = new Padding(5, 5, 5, 5)
                            };
                            button.Click += eventHandler;
                            ToolTip toolTip = new ToolTip {
                                AutoPopDelay = 5000,
                                InitialDelay = 1000,
                                ReshowDelay = 500,
                                ShowAlways = true
                            };
                            if (type == TRANSFER_SHORTCUT_TYPE) {
                                toolTip.SetToolTip(button, "Transfer");
                                var menuItem = new ToolStripMenuItem() { Text = button.Text, Tag = shortcut.ToString() };
                                menuItem.Click += eventHandler;
                                actionsContextMenuStrip.Items.Add(menuItem);
                            } else if (type == CALL_SHORTCUT_TYPE) {
                                toolTip.SetToolTip(button, "Call");
                            }
                            shortcutPanel.Controls.Add(button);
                        }
                    }
                }
            }
            // интегрирую контакты в форму ContactsForm
            //var contactsJson = (string)jsonObj["contacts"];
            //if (String.IsNullOrEmpty(contactsJson) == false) {
            //    var contacts = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(contactsJson);
            //    if (contacts != null) {
            //        foreach (JToken contact in contacts) {
            //            Contacts.Add(new Tuple<string, string>(((JValue)contact["Name"]).Value.ToString(), ((JValue)contact["Number"]).Value.ToString()));
            //        }
            //    }
            //}

            //Apply modified config
            AbtoPhone.ApplyConfig();
            try {
                AbtoPhone.Initialize();
                displayNotifyMsg("SET_ACCOUNT");
            } catch (Exception e) {
                displayNotifyMsg(e.Message);
            }
        }

        private PhoneLine GetLineByButton(Button button) {
            return GetLineByButtonName(button.Name);
        }
        private PhoneLine GetLineByButtonName(string buttonName) {
            switch (buttonName) {
                case "actionsButton1":
                case "hungButton1":
                case "holdButton1":
                    return PhoneLinesHelper.GetLineByNumber(0);
                case "actionsButton2":
                case "hungButton2":
                case "holdButton2":
                    return PhoneLinesHelper.GetLineByNumber(1);
                case "actionsButton3":
                case "hungButton3":
                case "holdButton3":
                    return PhoneLinesHelper.GetLineByNumber(2);
                case "actionsButton4":
                case "hungButton4":
                case "holdButton4":
                    return PhoneLinesHelper.GetLineByNumber(3);
                default:
                    throw new Exception($"Undefined button '{buttonName}'");
            }
        }

        private Label GetTimeLabelByLine(int lineNumber) {
            return CallTimelabel(lineNumber);
        }

        private void MAKE_CALL_Action(string phone, string header, string subHeader, string loadId, object model) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => MAKE_CALL_Action(phone, header, subHeader, loadId, model)));
                return;
            }
            if (String.IsNullOrEmpty(phone)) {
                return;
            }

            var line = PhoneLinesHelper.ReserveLine(loadId == null);
            if (line != null) {
                line.Model = model;
                line.Phone = phone;

                //Set status
                displayNotifyMsg($"[{line.LineId}] Calling...");

                //Set flag, Cange controls state
                line.Calling = true;
                this.SetHeader(line.Number, header);
                this.SetPhone(line.Number, phone);
                this.SetSubHeader(line.Number, subHeader);
                this.SetLoadId(line.Number, loadId);

                //Start call and get assigned connectionId
                int connId = AbtoPhone.StartCall2(phone.OnlyNumbers());
                line.ConnectionIds.Add(connId);
                //int connId = AbtoPhone.StartCallAcc(address, acc2);
                CallService.Notification(abtophone_state.Connecting, phone, line.Number, line.Model);
                AddHistoryNumber(phone, header, model);

                UpdateShortcuts();

                if (this.WindowState == FormWindowState.Minimized) {
                    this.WindowState = FormWindowState.Normal;
                }
                //Task.Factory.StartNew(async () => {
                //    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                //    this.Invoke((Action)(() => this.Activate()));
                //}
                //);
                this.Activate();
            }
        }

        private void AddHistoryNumber(string phone, string header, object model) {
            if (String.IsNullOrEmpty(phone) == false) {
                phone = phone.ToPhone();
                if (numberComboBox.Items.Contains(phone)) {
                    numberComboBox.Items.Remove(phone);
                }
                numberComboBox.Items.Insert(0, phone);
                var maxItems = 10;
                if (numberComboBox.Items.Count > maxItems) {
                    for (int i = numberComboBox.Items.Count - 1; i >= maxItems; i--) {
                        numberComboBox.Items.RemoveAt(i);
                    }
                }
                HistoryCalls.Add(new Tuple<string, string, DateTime, object>(phone, header, DateTime.Now, model));
            }
        }

        private void numberComboBox_SelectedValueChanged(object sender, EventArgs e) {
            var number = ((ComboBox)sender).Text;
            if (String.IsNullOrEmpty(number) == false) {
                MAKE_CALL_Action(number, null, null, null, null);
            }
        }

        private void HANGUP_Action(string phone) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => HANGUP_Action(phone)));
                return;
            }
            var line = PhoneLinesHelper.GetLineByPhone(phone);
            if (line != null) {
                foreach (var connId in line.ConnectionIds) {
                    AbtoPhone.HangUp(connId);
                }
            } else {
                AbtoPhone.HangUpLastCall();
            }
        }

        private void HANGUP_ALL_Action(string phone) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => HANGUP_ALL_Action(phone)));
                return;
            }
            try {
                foreach (var connId in PhoneLinesHelper.GetAllConnectionIds()) {
                    AbtoPhone.HangUp(connId);
                }
            } catch (Exception e) {
                displayNotifyMsg(e.Message);
            }
        }

        private void TRANSFER_Action(string phone) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => TRANSFER_Action(phone)));
                return;
            }
            displayNotifyMsg("Err: Transfer was not implemented");
            //    //Get transfer addr
            //    TransferAddrForm dlg = new TransferAddrForm();
            //    if (dlg.ShowDialog(this) != DialogResult.OK) return;

            //    //Transfer call
            //    AbtoPhone.TransferCall(dlg.textBoxAddr.Text);
        }

        private async void ACTION_DTMF_Action(string phone, string dtmf) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => ACTION_DTMF_Action(phone, dtmf)));
                return;
            }
            var ln = PhoneLinesHelper.GetLineByPhone(phone);
            if (ln != null) {
                try {
                    PhoneLabel(ln.Number).Text = ln.Phone.SetDtmf(dtmf);
                    foreach (var ch in dtmf) {
                        var dtmfNum = ch.ToString();
                        playDtmf(ref dtmfNum);
                        AbtoPhone.SendToneEx(Convert.ToInt32(dtmfNum), 200, 0, 1, 0);
                        displayNotifyMsg($"Send tone: {dtmfNum}");
                        await Task.Delay(TimeSpan.FromSeconds(0.3));
                    }
                } catch (Exception ex) {
                    displayNotifyMsg(ex.Message);
                }
            }
        }

        private void HOLD_Action(string phone) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => HOLD_Action(phone)));
                return;
            }
            var ln = PhoneLinesHelper.GetLineByPhone(phone);
            if (ln != null) {
                //Hold/Retrieve
                if (ln.CallHeld == false && ln.LineId != null) {
                    AbtoPhone.HoldCall(ln.LineId.Value);
                }
            }
        }

        private void RESUME_Action(string phone) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => RESUME_Action(phone)));
                return;
            }
            var ln = PhoneLinesHelper.GetLineByPhone(phone);
            if (ln != null) {
                //Hold/Retrieve
                if (ln.CallHeld && ln.LineId != null) {
                    AbtoPhone.RetrieveCall(ln.LineId.Value);
                }
            }
        }

        #endregion [ AutoCall Actions ]

        #region [ Event Handlers ]

        private Button GetDtmfByCode(Keys key) {
            switch (key) {
                case Keys.D0:
                case Keys.NumPad0:
                    return DTFM0;
                case Keys.D1:
                case Keys.NumPad1:
                    return DTFM1;
                case Keys.D2:
                case Keys.NumPad2:
                    return DTFM2;
                case Keys.D3:
                case Keys.NumPad3:
                    return DTFM3;
                case Keys.D4:
                case Keys.NumPad4:
                    return DTFM4;
                case Keys.D5:
                case Keys.NumPad5:
                    return DTFM5;
                case Keys.D6:
                case Keys.NumPad6:
                    return DTFM6;
                case Keys.D7:
                case Keys.NumPad7:
                    return DTFM7;
                case Keys.D8:
                case Keys.NumPad8:
                    return DTFM8;
                case Keys.D9:
                case Keys.NumPad9:
                    return DTFM9;
                case Keys.Multiply:
                    return DTFMStar;
                default:
                    return null;
            }
        }

        private void NumberComboBox_GotFocus(object sender, EventArgs e) {
            this.Focus();
        }

        private void numberComboBox_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void SIPPhoneForm_KeyDown(object sender, KeyEventArgs e) {
            var control = GetDtmfByCode(e.KeyCode);
            if (control != null) {
                control.Focus();
            }
        }

        private void SIPPhoneForm_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Back) {
                if (String.IsNullOrEmpty(numberComboBox.Text) == false) {
                    numberComboBox.Text = numberComboBox.Text.Remove(numberComboBox.Text.Length - 1, 1);
                }
                return;
            }
            if (e.KeyCode == Keys.Enter) {
                callBtn_Click(sender, e);
                return;
            }
            var control = GetDtmfByCode(e.KeyCode);
            if (control != null) {
                control.PerformClick();
                this.ActiveControl = null;
                DTMF_MouseDown(control, null);
            }
        }

        private void hungBtn_Click(object sender, EventArgs e) {
            var ln = GetLineByButton((Button)sender);

            if (ln != null) {
                if (ln.CallEstablished || ln.Calling) {
                    //HangUp
                    foreach (var connId in ln.ConnectionIds) {
                        AbtoPhone.HangUp(connId);
                    }
                    CallService.Notification(abtophone_state.Ended, ln.Phone, ln.Number, ln.Model);
                }
                // принудительно убираем контрол линии
                PhoneLinesHelper.Reset(ln.Number);
                UpdateShortcuts();
            }
            this.numberComboBox.Text = String.Empty;
        }

        private void callBtn_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                callBtn_Click(sender, e);
            }
        }

        private void callBtn_Click(object sender, EventArgs e) {
            var ln = PhoneLinesHelper.ReserveLine(true);
            if (ln != null) {
                //Get addr
                string number = numberComboBox.Text;
                if (number.Length == 0) return;
                //Set status
                displayNotifyMsg($"[{ln.LineId}] Calling...");

                //Set flag, Cange controls state
                ln.Calling = true;
                ln.Phone = number;
                this.SetHeader(ln.Number, String.Empty);
                this.SetPhone(ln.Number, number);
                numberComboBox.Text = null;

                //Start call and get assigned connectionId		
                //		
                int connId = AbtoPhone.StartCall2(number.OnlyNumbers());
                ln.ConnectionIds.Add(connId);
                AddHistoryNumber(number, null, null);
            }
        }

        private void holdRetreiveBtn_Click(object sender, EventArgs e) {
            var ln = GetLineByButton((Button)sender);

            //Hold/Retrieve
            if (ln.LineId != null) {
                if (ln.CallHeld) {
                    AbtoPhone.RetrieveCall(ln.LineId.Value);
                } else {
                    AbtoPhone.HoldCall(ln.LineId.Value);
                }
            }
        }

        private void linePanel_MouseDoubleClick(object sender, MouseEventArgs e) {
            Control panel;
            if (sender is Panel) {
                panel = (Control)sender;
            } else {
                panel = ((Control)sender).Parent;
            }
            CallService.Notification(abtophone_state.DETAILS, panel.Tag as String, null, null);
        }

        private void transferBtn_Click(object sender, EventArgs arg) {
            //Get transfer addr
            TransferAddrForm dlg = new TransferAddrForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var line = PhoneLinesHelper.GetAvailableTransferLine();
            if (line != null && line.LineId != null) {
                //Transfer call
                AbtoPhone.SetCurrentLine(line.LineId.Value);
                AbtoPhone.TransferCall(dlg.textBoxAddr.Text);
                return;
            }
            // отложенный трансфер
            if (line == null) {
                line = PhoneLinesHelper.GetLine(null);
                if (line == null) {
                    return;
                }
            }
            line.TransferPhone = dlg.textBoxAddr.Text;
        }

        private void autoTransferShortcutBtn_Click(object sender, EventArgs args) {
            var json = (string)(sender is Control ? ((Control)sender).Tag : ((ToolStripMenuItem)sender).Tag);
            var jsonObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var number = (string)((JValue)jsonObj["Number"]).Value;
            PhoneLine line = null;
            if (sender is Button) {
                line = PhoneLinesHelper.GetAvailableTransferLine();
            } else {
                line = GetLineByButtonName(this.ButtonName);
            }
            if (line != null && line.LineId != null) {
                //Transfer call
                line.TransferPhone = number;
                AbtoPhone.SetCurrentLine(line.LineId.Value);
                AbtoPhone.TransferCall(number);
                return;
            }
            // отложенный трансфер
            if (line == null) {
                line = PhoneLinesHelper.GetLine(null);
                if (line == null) {
                    return;
                }
            }
            line.TransferPhone = number;
            SetEnabledShortcutAutoTransfer(false);
        }

        private void callShortcutBtn_Click(object sender, EventArgs e) {
            var json = (string)((Button)sender).Tag;
            var jsonObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var number = (string)((JValue)jsonObj["Number"]).Value;
            var label = (string)((JValue)jsonObj["Label"]).Value;
            MAKE_CALL_Action(number, label, null, null, null);
        }

        private void DTFM_Click(object sender, EventArgs args) {
        }

        private void DTMF_MouseClick(object sender, MouseEventArgs e) {
        }

        private void DTMF_MouseDown(object sender, MouseEventArgs e) {
            Button b = (Button)sender;
            var dtmf = (string)b.Tag;
            playDtmf(ref dtmf);

            try {
                var ln = PhoneLinesHelper.GetActiveLine();
                if (ln != null && ln.LineId != null) {
                    if (!(ln.CallEstablished || ln.Calling)) {
                    } else {
                        AbtoPhone.SetCurrentLine(ln.LineId.Value);
                        AbtoPhone.SendToneEx(Convert.ToInt32(b.Tag), 200, 0, 1, 0);
                        PhoneLabel(ln.Number).Text = PhoneLabel(ln.Number).Text.AddDtmf(dtmf);
                    }
                } else if (PhoneLinesHelper.GetAvailableAny() == false) {
                    this.numberComboBox.Text += dtmf;
                    this.numberComboBox.SelectionStart = this.numberComboBox.Text.Length;
                }
            } catch (Exception ex) {
                displayNotifyMsg(ex.Message);
            }
        }

        private void SpkVolume_Scroll(object sender, EventArgs e) {
            AbtoPhone.PlaybackVolume = spkVolume.Value;
            SetVolume(spkVolume.Value, !muteSpeakerFlag.Checked);
        }

        private void muteSoundCB_CheckedChanged(object sender, EventArgs e) {
            AbtoPhone.PlaybackMuted = muteSpeakerFlag.Checked ? 0 : 1;
            muteSpeakerFlag.Image = muteSpeakerFlag.Checked ? Properties.Resources.muteout : Properties.Resources.mutedout;
            SetVolume(spkVolume.Value, !muteSpeakerFlag.Checked);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            //Get cfg values
            CConfig phoneCfg = AbtoPhone.Config;

            //Create and show dlg
            ConfigForm cfgDlg = new ConfigForm();
            cfgDlg.phoneCfg = phoneCfg;
            cfgDlg.m_defLoadStorePath = AbtoPhone.SDKPath();
            cfgDlg.m_versionStr = AbtoPhone.RetrieveVersion();
            cfgDlg.m_addrStr = AbtoPhone.RetrieveExternalAddress();

            if (cfgDlg.ShowDialog(this) != DialogResult.OK) {
                AbtoPhone.CancelConfig();
                return;
            }

            //Apply new values
            AbtoPhone.ApplyConfig();

            //Store
            phoneCfg.Store(cfgFileName);

            //Get state
            m_MP3RecordingEnabled = phoneCfg.MP3RecordingEnabled != 0;
            m_AutoAnswerEnabled = phoneCfg.AutoAnswerEnabled != 0;
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e) {
            HistoryForm hstDlg = new HistoryForm();
            hstDlg.SetSource(HistoryCalls);
            hstDlg.ShowDialog();
            if (hstDlg.SelectedHistoryCall != null) {
                MAKE_CALL_Action(
                    hstDlg.SelectedHistoryCall.Number,
                    hstDlg.SelectedHistoryCall.Header,
                    null, null,
                    hstDlg.SelectedHistoryCall.Model);
            }
            if (hstDlg.SelectedHistoryDetals != null && hstDlg.SelectedHistoryDetals.Model != null) {
                CallService.Notification(abtophone_state.DETAILS, null, null, hstDlg.SelectedHistoryDetals.Model);
            }
        }

        private void contactsToolStripMenuItem_Click(object sender, EventArgs e) {
            ContactsForm ctcDlg = new ContactsForm(CallService);
            ctcDlg.ShowDialog();
            if (ctcDlg.SelectedContact != null) {
                MAKE_CALL_Action(ctcDlg.SelectedContact.Number, ctcDlg.SelectedContact.Header, null, null, null);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            AboutForm dlg = new AboutForm();
            dlg.ShowDialog(this);
        }

        private void webPageLabel_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo("http://voipsipsdk.com") { UseShellExecute = true });
        }

        private void activateSDK_Click(object sender, EventArgs e) {
            //Show form
            ActivationForm dlg = new ActivationForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            //Store cfg values
            CConfig phoneCfg = AbtoPhone.Config;
            phoneCfg.LicenseUserId = dlg.m_licUserId;
            phoneCfg.LicenseKey = dlg.m_licKey;
            phoneCfg.Store(cfgFileName);
            //Message box
            MessageBox.Show("Restart application for finalize activation.");
        }

        private bool showLogs = false;

        private void notificationsListBox_MouseClick(object sender, MouseEventArgs e) {
            showLogs = !showLogs;
            var listBox = notificationsListBox;
            if (showLogs) {
                listBox.Height = 119;
            } else {
                listBox.Height = 15;
            }
            int visibleItems = listBox.ClientSize.Height / listBox.ItemHeight;
            listBox.TopIndex = Math.Max(listBox.Items.Count - visibleItems + 1, 0);
        }

        #endregion [ Event Handlers ]

        #region [ Sounds ]

        private async void LoadMedia() {
            //this.SetVolume(0, true);
            //this.LoadMedia(sound0, "0.wav", Properties.Resources._0);
            //this.LoadMedia(sound1, "1.wav", Properties.Resources._1);
            //this.LoadMedia(sound2, "2.wav", Properties.Resources._2);
            //this.LoadMedia(sound3, "3.wav", Properties.Resources._3);
            //this.LoadMedia(sound4, "4.wav", Properties.Resources._4);
            //this.LoadMedia(sound5, "5.wav", Properties.Resources._5);
            //this.LoadMedia(sound6, "6.wav", Properties.Resources._6);
            //this.LoadMedia(sound7, "7.wav", Properties.Resources._7);
            //this.LoadMedia(sound8, "8.wav", Properties.Resources._8);
            //this.LoadMedia(sound9, "9.wav", Properties.Resources._9);
            //this.LoadMedia(soundStar, "star.wav", Properties.Resources.star);
            //this.LoadMedia(soundHash, "hash.wav", Properties.Resources.hash);
            //await Task.Delay(TimeSpan.FromSeconds(1));
            //this.SetVolume(spkVolume.Value, !muteSpeakerFlag.Checked);
        }

        private void LoadMedia(MediaPlayer mp, string fileName, Stream stream) {
            var tmpDirName = "SIPVoipSDK64";
            var path = Path.Combine(Path.GetTempPath(), tmpDirName, fileName);
            if (Directory.Exists(tmpDirName) == false) {
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), tmpDirName));
            }
            if (File.Exists(path) == false) {
                File.WriteAllBytes(path, BinaryHelper.ReadFully(stream));
            }
            mp.Open(new Uri(path));
        }
        private Task task;
        private double oldVolume = 0;
        private async void SetVolume(int volume, bool mute) {
            var valueD = (double)volume / 100;
            if (muteSpeakerFlag.Checked == false && valueD > oldVolume) {
                mute = false;
                muteSpeakerFlag.Checked = true;
                muteSoundCB_CheckedChanged(muteSpeakerFlag, new EventArgs());
            }

            #region [ single request + delay ]
            var localTask = task = Task.Delay(TimeSpan.FromMilliseconds(100));
            await task;
            if (localTask != task) {
                return;
            }
            #endregion [ single request + delay ]

            if (oldVolume != 0) {
                var dtmf = "11";
                playDtmf1(ref dtmf);
            }

            #region [ single request + delay ]
            await task;
            #endregion [ single request + delay ]

            oldVolume = valueD;

            //sound0.IsMuted = mute;
            //sound1.IsMuted = mute;
            //sound2.IsMuted = mute;
            //sound3.IsMuted = mute;
            //sound4.IsMuted = mute;
            //sound5.IsMuted = mute;
            //sound6.IsMuted = mute;
            //sound7.IsMuted = mute;
            //sound8.IsMuted = mute;
            //sound9.IsMuted = mute;
            //soundStar.IsMuted = mute;
            //soundHash.IsMuted = mute;

            //sound0.Volume = valueD;
            //sound1.Volume = valueD;
            //sound2.Volume = valueD;
            //sound3.Volume = valueD;
            //sound4.Volume = valueD;
            //sound5.Volume = valueD;
            //sound6.Volume = valueD;
            //sound7.Volume = valueD;
            //sound8.Volume = valueD;
            //sound9.Volume = valueD;
            //soundStar.Volume = valueD;
            //soundHash.Volume = valueD;
        }

        private void playDtmf(ref string dtmf) {
            //if (PhoneLinesHelper.GetAvailableTransferLine() != null) {
            //    playDtmf2(ref dtmf);
            //} else {
            playDtmf1(ref dtmf);
            //}
        }
        MediaPlayer mp = null;
        private void playDtmf1(ref string dtmf) {
            var file = dtmf;
            switch (dtmf) {
                case "0":
                    break;
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
                case "4":
                    break;
                case "5":
                    break;
                case "6":
                    break;
                case "7":
                    break;
                case "8":
                    break;
                case "9":
                    break;
                case "10":
                    dtmf = "*";
                    file = "star";
                    break;
                case "11":
                    dtmf = "#";
                    file = "hash";
                    break;
            }
            if (mp == null) {
                mp = new MediaPlayer();
            }
            mp.Open(new Uri(getSoundFilePath($"{file}.wav")));

            var volume = spkVolume.Value;
            var mute = !muteSpeakerFlag.Checked;
            var valueD = (double)volume / 100;

            mp.IsMuted = mute;
            mp.Volume = valueD;

            mp.Play();
        }

        //private void playDtmf2(ref string dtmf) {
        //    var file = dtmf;
        //    switch (dtmf) {
        //        case "0":
        //            break;
        //        case "1":
        //            break;
        //        case "2":
        //            break;
        //        case "3":
        //            break;
        //        case "4":
        //            break;
        //        case "5":
        //            break;
        //        case "6":
        //            break;
        //        case "7":
        //            break;
        //        case "8":
        //            break;
        //        case "9":
        //            break;
        //        case "10":
        //            dtmf = "*";
        //            file = "star";
        //            break;
        //        case "11":
        //            dtmf = "#";
        //            file = "hash";
        //            break;
        //    }
        //    AbtoPhone.PlayFile(getSoundFilePath($"{file}.wav"));
        //}

        #endregion [ Sounds ]

        #region [ SDK Events ]

        delegate void ActivateSDK_Delegate(object sender, EventArgs e);

        private void AbtoPhone_OnHoldCall(int lineId, int isHeld) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnHoldCall(lineId, isHeld)));
                return;
            }
            var ln = PhoneLinesHelper.GetLine(lineId);
            if (isHeld == 0) {
                // на холд остальные
                foreach (var line in PhoneLinesHelper.GetAvailableTransferLines()) {
                    AbtoPhone.HoldCall(line.LineId.Value);
                }
                displayNotifyMsg($"SetCurrentLine3({ln.LineId})");
                AbtoPhone.SetCurrentLine(ln.LineId.Value);
                CallService.Notification(abtophone_state.Connected, ln.Phone, ln.Number, ln.Model);
                ln.CallHeld = false;
                UpdateShortcuts();
                displayNotifyMsg($"[{lineId}] call retrieved");
            } else if (isHeld == 1) {
                CallService.Notification(abtophone_state.Hold, ln.Phone, ln.Number, ln.Model);
                ln.CallHeld = true;
                UpdateShortcuts();
                displayNotifyMsg($"[{lineId}] call is held");
            } else {
                displayNotifyMsg($"[{lineId}] ERR: undefined hold status '{isHeld}'");
            }
        }

        private void AbtoPhone_OnInitialized(string Msg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnInitialized(Msg)));
                return;
            }
            displayNotifyMsg(Msg);
            if (Msg.Contains("FAILED")) {
                BeginInvoke(new ActivateSDK_Delegate(activateSDK_Click), this, null);
            }
        }

        private void AbtoPhone_OnLineSwiched(int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnLineSwiched(lineId)));
                return;
            }
            displayNotifyMsg($"line switched [{lineId}]");
        }

        private void AbtoPhone_OnEstablishedCall(string msg, int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnEstablishedCall(msg, lineId)));
                return;
            }
            //Update line state
            var ln = PhoneLinesHelper.GetLine(lineId);
            if (ln == null) {
                AbtoPhone.HangUpCallLine(lineId);
                displayNotifyMsg($"Err: Max Lines [{lineId}]");
                return;
            }
            // если есть активная линия, то новую дозвонивщуюся на холд
            var activeLines = PhoneLinesHelper.GetActiveLines();
            if (activeLines.Any(e => e.LineId != lineId)) {
                AbtoPhone.HoldCall(lineId);
                var activeLine = activeLines.First();
                displayNotifyMsg($"SetCurrentLine2({activeLine.LineId})");
                AbtoPhone.SetCurrentLine(activeLine.LineId.Value);
            }
            ln.CallEstablished = true;
            ln.Calling = false;
            //Start call duration timer
            ln.StartCallDurationTimer((time) => this.GetTimeLabelByLine(ln.Number).Text = time);
            //Display status
            displayNotifyMsg($"[{lineId}]: {msg}");
        }

        //This variable allow to establish only one call and reject others
        //int gCallsCounter = 0;

        void AbtoPhone_OnIncomingCall2(string AddrFrom, string AddrTo, int connectionId, int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnIncomingCall2(AddrFrom, AddrTo, connectionId, lineId)));
                return;
            }
            if (m_AutoAnswerEnabled == true) return;

            //string sipMsg = AbtoPhone.GetSIPMessageLine(lineId);
            displayNotifyMsg("OnIncomingCall2 line=" + lineId.ToString());

            //Show form
            IncomingForm dlg = new IncomingForm();
            dlg.textBoxCaller.Text = AddrFrom;
            dlg.textBoxLine.Text = "Line" + lineId.ToString();

            //Answer audio or video
            if (dlg.ShowDialog(this) == DialogResult.Yes) {
                var ln = PhoneLinesHelper.GetLine(lineId);
                AbtoPhone.AnswerCallLine(lineId);
            } else {
                AbtoPhone.RejectCallLine(lineId);
            }
        }

        private void AbtoPhone_OnClearedCall(string Msg, int status, int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnClearedCall(Msg, status, lineId)));
                return;
            }
            //Update line state
            var ln = PhoneLinesHelper.GetLine(lineId);
            //
            if (ln != null) {
                PhoneLinesHelper.Reset(ln.Number);

                //Update controls (only when it's cur line event)
                displayNotifyMsg($"[{lineId}]: {Msg}. Status: {status}");

                if (ln.TransferPhone != null) {
                    CallService.Notification(abtophone_state.Transferred, ln.Phone, ln.Number, ln.Model);
                } else {
                    CallService.Notification(abtophone_state.Ended, ln.Phone, ln.Number, ln.Model);
                }
                UpdateShortcuts();
            }
        }

        private void AbtoPhone_OnRegistered(string Msg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnRegistered(Msg)));
                return;
            }
            if (Msg.Contains("Registration removed:")) {
                //Unregistered
            }

            CConfig phoneCfg = AbtoPhone.Config;
            if (Msg.Contains("Registration removed")) {
                //toolStripStatusLabel1.Text = "Unknown";
                //this.toolStripStatusLabel1.Image = Properties.Resources.unknown;
            } else if (Msg.Contains("Registration success")) {
                toolStripStatusLabel1.Text = $"Online {phoneCfg.RegUser}";
                this.toolStripStatusLabel1.Image = Properties.Resources.online;
            } else if (Msg.Contains("Registration failure")) {
                toolStripStatusLabel1.Text = "Offline";
                this.toolStripStatusLabel1.Image = Properties.Resources.offline;
            }

            displayNotifyMsg(Msg);
        }

        private void AbtoPhone_OnPlayFinished2(string Msg, int LineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnPlayFinished2(Msg, LineId)));
                return;
            }
            stopStartPlaying(true, LineId);
            displayNotifyMsg(Msg);
        }

        private void stopStartPlaying(bool bCalledByPlayFinishedEvent, int lineId) {
        }

        private void AbtoPhone_OnEstablishedConnection(string addrFrom, string addrTo, int connectionId, int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnEstablishedConnection(addrFrom, addrTo, connectionId, lineId)));
                return;
            }
            if (String.IsNullOrEmpty(RegisterName) == false && addrTo.Contains(RegisterName)) {
                var ln = PhoneLinesHelper.ReserveLine(lineId);
                ln.Phone = addrFrom.OnlyNumbers();
                ln.Connections.Add(connectionId, addrFrom);
                this.SetHeader(ln.Number, "Incoming Call");
                this.SetPhone(ln.Number, ln.Phone);
                return;
            }

            {
                var ln = PhoneLinesHelper.GetLineByConnectionId(connectionId);
                if (ln == null) {
                    AbtoPhone.HangUpCallLine(lineId);
                    return;
                } else {
                    ln.LineId = lineId;
                    if (ln.TransferPhone != null) {
                        //Transfer call
                        AbtoPhone.SetCurrentLine(lineId);
                        AbtoPhone.TransferCall(ln.TransferPhone);
                        return;
                    }
                }
                string addr = ln.Calling ? addrTo : addrFrom;
                ln.Connections.Add(connectionId, addr);
                if (ln.CallHeld) {
                    CallService.Notification(abtophone_state.Hold, ln.Phone, ln.Number, ln.Model);
                    UpdateShortcuts();
                } else {
                    CallService.Notification(abtophone_state.Connected, ln.Phone, ln.Number, ln.Model);
                    UpdateShortcuts();
                }
            }
        }

        private void AbtoPhone_OnClearedConnection(int connectionId, int lineId) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnClearedConnection(connectionId, lineId)));
                return;
            }
            var ln = PhoneLinesHelper.GetLineByConnectionId(connectionId);
            if (ln == null) {
                return;
            }
            ln.Connections.Remove(connectionId);
            if (ln.Connections.Any() == false) {
                PhoneLinesHelper.Reset(ln.Number);
                if (ln.TransferPhone != null) {
                    CallService.Notification(abtophone_state.Transferred, ln.Phone, ln.Number, ln.Model);
                } else {
                    CallService.Notification(abtophone_state.Ended, ln.Phone, ln.Number, ln.Model);
                }
            }
            UpdateShortcuts();
        }

        private void AbtoPhone_OnTextMessageReceived(string from, string message) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnTextMessageReceived(from, message)));
                return;
            }
            displayNotifyMsg("'" + message + "' received from: " + from);
        }

        private void AbtoPhone_OnTextMessageSentStatus(string address, string reason, int bSuccess) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnTextMessageSentStatus(address, reason, bSuccess)));
                return;
            }
            if (bSuccess != 0) displayNotifyMsg("Message sent successfully to " + address + " Reason: " + reason);
            else displayNotifyMsg("Message sent failure to " + address + " Reason: " + reason);
        }

        private void AbtoPhone_OnPhoneNotify(string message) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnPhoneNotify(message)));
                return;
            }
            displayNotifyMsg(message);

            //"Redirect Success. Connection: x";
            //"Redirect Failure. Connection: x Status y";
            {
                Match match = Regex.Match(message, @"Redirect.*Connection: \d+");
                if (match.Success) {
                    string connIdStr = Regex.Match(match.Value, @"\d+").Value;
                    AbtoPhone.HangUp(int.Parse(connIdStr));
                }
            }
            //"Registered as: 'sip:0002@phone.myautocall.net'"
            {
                Match match = Regex.Match(message, @".+'(?<name>.+)'");
                if (match.Success) {
                    RegisterName = match.Groups["name"].Value;
                }
            }
        }

        private void AbtoPhone_OnRemoteAlerting2(int ConnectionId, int lineid, int responseCode, string reasonMsg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnRemoteAlerting2(ConnectionId, lineid, responseCode, reasonMsg)));
                return;
            }
            string str = "Remote alerting: " + responseCode.ToString() + " " + reasonMsg;
            displayNotifyMsg(str);
        }

        void AbtoPhone_OnHoldCall2(int lineId, int ConnectionId, bool isPutOnHold, bool isRemote) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnHoldCall2(lineId, ConnectionId, isPutOnHold, isRemote)));
                return;
            }
            displayNotifyMsg(string.Format("isPutOnHold = {0} isRemote={1}", isPutOnHold, isRemote));

            var ln = PhoneLinesHelper.GetLine(lineId);
            if (ln.Connections.Any()) {
                if (isPutOnHold) {
                    CallService.Notification(abtophone_state.Hold, ln.Phone, ln.Number, ln.Model);
                    UpdateShortcuts();
                } else {
                    CallService.Notification(abtophone_state.Connected, ln.Phone, ln.Number, ln.Model);
                    UpdateShortcuts();
                }
            }
        }

        void AbtoPhone_OnRecordFinished2(int ConnectionId, int lineId, long bufferPtr) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnRecordFinished2(ConnectionId, lineId, bufferPtr)));
                return;
            }
            displayNotifyMsg("RecordFinished connId=" + ConnectionId + " lineId=" + lineId + " bufPtr=" + bufferPtr);
        }

        void AbtoPhone_OnRecordFinished(string Msg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnRecordFinished(Msg)));
                return;
            }
            displayNotifyMsg(Msg);
        }

        void AbtoPhone_OnSubscribeStatus(int subscriptionId, int statusCode, string statusMsg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnSubscribeStatus(subscriptionId, statusCode, statusMsg)));
                return;
            }
            string str = string.Format("OnVoiceMail: Not supported. StatusCode: {0}", statusCode);
            displayNotifyMsg(str);
        }

        void AbtoPhone_OnSubscriptionNotify(int subscriptionId, string StateStr, string NotifyStr) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnSubscriptionNotify(subscriptionId, StateStr, NotifyStr)));
                return;
            }
            string str = string.Format("OnVoiceMail: {0}", StateStr);
            displayNotifyMsg(str);
        }

        void AbtoPhone_OnReceivedRequestUpdate(int ConnectionId, int LineId, string sipMsg) {
            if (this.InvokeRequired) {
                if (Thread.CurrentThread.IsAlive == false) return;
                this.Invoke((Action)(() => AbtoPhone_OnReceivedRequestUpdate(ConnectionId, LineId, sipMsg)));
                return;
            }
            displayNotifyMsg(sipMsg);
        }

        #endregion [ SDK Events ]

        #region [ Action Button Menu ]

        private string ButtonName = null;
        private void actionsButton_Click(object sender, EventArgs e) {
            if (ButtonName == null) {
                Button btnSender = (Button)sender;
                ButtonName = btnSender.Name;
                Point ptLowerLeft = new Point(0, btnSender.Height);
                ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
                actionsContextMenuStrip.Show(ptLowerLeft);
            }
        }

        private void actionsContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            Task.Factory.StartNew(async () => {
                await Task.Delay(TimeSpan.FromSeconds(0.2));
                ButtonName = null;
            });
        }

        #endregion [ Action Button Menu ]

        #region [ Register SDK ]

        public void ExternalExceptionHandler(Exception e) {
            if ((e is ExternalException && ((ExternalException)e).ErrorCode == -2147221164) ||
                (e is FileNotFoundException && ((FileNotFoundException)e).HResult == -2147024770)) {
                try {
                    RegisterSIPVoipSDK32();
                    this.Close();
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
                return;
            } else {
                Console.WriteLine("MyHandler caught : " + e.Message);
            }
        }

        private void RegisterSIPVoipSDK32() {
            var destinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIPVoipSDK");
            var proc = new ProcessStartInfo();
            proc.Verb = "runas";
            proc.FileName = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "cmd.exe");
            proc.UseShellExecute = true;
            proc.WorkingDirectory = destinPath;
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Arguments = $"/C cd {destinPath}&regsvr32 /s SIPVoipSDK.dll";
            var p = Process.Start(proc);
            p.WaitForExit();
        }

        #endregion [ Register SDK ]
    }
}
