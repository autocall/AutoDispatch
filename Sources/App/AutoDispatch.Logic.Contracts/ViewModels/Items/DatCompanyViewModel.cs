using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AutoCall.Common.Collections;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class DatCompanyViewModel : BaseObjectViewModel {

        private Guid? _actualCompanyId;
        private Dictionary<string, string> _fields;
        private DateTime? _insuranceExpiration;
        private ContactsViewModel _contacts;
        private bool _phy_UpsStore;
        private bool _mai_UpsStore;
        private bool _phy_MailBox;
        private bool _mai_MailBox;
        private DatCompanyType _entityType;
        private int? _mcNumber;
        private int? _dotNumber;
        private string _name;
        private string _insuranceFilePath;
        private byte[] _insuranceFileData;
        private bool _insuranceExpirationChanged;
        private string _comments;
        private byte _rating;
        private bool _setupPacketSent;
        private bool _setupPacketCompleted;
        private string _setupPacketFile;
        private string _w9File;
        private bool _setupPacketAccept;
        private ObservableCollection<Wrapper1LoadViewModel> _loads = new ObservableCollection<Wrapper1LoadViewModel>();
        private string _hyperlinkFile;

        public ObservableCollection<Wrapper1LoadViewModel> Loads {
            get => _loads;
            set {
                _loads = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        ///     Broker Company ID </summary>
        public Guid? ActualCompanyId {
            get { return _actualCompanyId; }
            set {
                if (_actualCompanyId != value) {
                    _actualCompanyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? McNumber {
            get { return _mcNumber; }
            set {
                if (_mcNumber != value) {
                    _mcNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? DotNumber {
            get { return _dotNumber; }
            set {
                if (_dotNumber != value) {
                    _dotNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public DatCompanyType EntityType {
            get { return _entityType; }
            set {
                if (_entityType != value) {
                    _entityType = value;
                    OnPropertyChanged();
                }
            }
        }

        public ContactsViewModel Contacts {
            get { return _contacts; }
            set {
                if (_contacts != value) {
                    _contacts = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ContactsString");
                }
            }
        }

        public string ContactsString => Contacts.ToString();

        public bool Phy_UpsStore {
            get { return _phy_UpsStore; }
            set {
                if (_phy_UpsStore != value) {
                    _phy_UpsStore = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Mai_UpsStore {
            get { return _mai_UpsStore; }
            set {
                if (_mai_UpsStore != value) {
                    _mai_UpsStore = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Phy_MailBox {
            get { return _phy_MailBox; }
            set {
                if (_phy_MailBox != value) {
                    _phy_MailBox = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Mai_MailBox {
            get { return _mai_MailBox; }
            set {
                if (_mai_MailBox != value) {
                    _mai_MailBox = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? InsuranceExpiration {
            get { return _insuranceExpiration; }
            set {
                if (_insuranceExpiration != value) {
                    _insuranceExpiration = value;
                    InsuranceExpirationChanged = true;
                    OnPropertyChanged();
                    OnPropertyChanged("InsuranceState");
                }
            }
        }

        public string InsuranceFilePath {
            get { return _insuranceFilePath; }
            set {
                if (_insuranceFilePath != value) {
                    _insuranceFilePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged("InsuranceState");
                }
            }
        }

        public byte[] InsuranceFileData {
            get { return _insuranceFileData; }
            set {
                if (_insuranceFileData != value) {
                    _insuranceFileData = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool InsuranceExpirationChanged {
            get { return _insuranceExpirationChanged; }
            set {
                _insuranceExpirationChanged = value;
                OnPropertyChanged();
            }
        }

        public InsuranceStateType InsuranceState {
            get {
                if (InsuranceFilePath == null || InsuranceExpiration == null) {
                    return InsuranceStateType.Gray;
                }
                if (DateTime.Now > InsuranceExpiration) {
                    return InsuranceStateType.Red;
                }
                if (InsuranceExpiration - DateTime.Now < TimeSpan.FromDays(14)) {
                    return InsuranceStateType.Yellow;
                }
                return InsuranceStateType.Green;
            }
        }

        public byte Rating {
            get { return _rating; }
            set {
                if (_rating != value) {
                    _rating = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HyperlinkFile {
            get => _hyperlinkFile;
            set {
                if (_hyperlinkFile != value) {
                    _hyperlinkFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HyperlinkFileShort));
                }
            }
        }

        public Visibility RatingVisibility => Rating != 0 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        ///     [Label, Field] from HTML </summary>
        public Dictionary<string, string> Fields {
            get { return _fields; }
            set {
                if (_fields != value) {
                    _fields = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ContactNames));
                }
            }
        }

        public string ContactNames {
            get {
                if (this.Fields != null) {
                    var COMPANY_REP1 = this.Fields.ContainsKey("COMPANY_REP1") ? this.Fields["COMPANY_REP1"] : null;
                    var COMPANY_REP2 = this.Fields.ContainsKey("COMPANY_REP2") ? this.Fields["COMPANY_REP2"] : null;
                    var contacts = new List<string>() { COMPANY_REP1, COMPANY_REP2 }.Where(e => e != null).Distinct().ToList();
                    return String.Join(", ", contacts);
                }
                return null;
            }
        }

        #region [ setup packet ]

        public Guid? SetupPacketId { get; set; }

        public List<string> SetupPacketEmailSent { get; set; }

        public bool SetupPacketSent {
            get { return _setupPacketSent; }
            set {
                if (_setupPacketSent != value) {
                    _setupPacketSent = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SetupPacketCompleted {
            get { return _setupPacketCompleted; }
            set {
                if (_setupPacketCompleted != value) {
                    _setupPacketCompleted = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsNew");
                }
            }
        }

        public bool SetupPacketAccept {
            get { return _setupPacketAccept; }
            set {
                if (_setupPacketAccept != value) {
                    _setupPacketAccept = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsNew");
                }
            }
        }

        public string SetupPacketFile {
            get { return _setupPacketFile; }
            set {
                if (_setupPacketFile != value) {
                    _setupPacketFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public string W9File {
            get { return _w9File; }
            set {
                if (_w9File != value) {
                    _w9File = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNew {
            get { return SetupPacketCompleted && !SetupPacketAccept; }
        }

        public SetupPacketViewModel SetupPacket => DispatchBoardViewModel.Instance.SetupPackets.FirstOrDefault(e => e.EntityId == this.SetupPacketId);

        public bool DoNotUse {
            get => doNotUse;
            set {
                if (doNotUse != value) {
                    doNotUse = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ setup packet ]

        public string HyperlinkFileShort {
            get {
                if (HyperlinkFile == null) {
                    return null;
                }
                if (HyperlinkFile.Contains(".")) {
                    var index = HyperlinkFile.LastIndexOf('.');
                    return HyperlinkFile.Substring(0, index).Truncate(13) + ".." + HyperlinkFile.Substring(index, HyperlinkFile.Length - index).Truncate(16);
                } else {
                    return HyperlinkFile.Truncate(16);
                }
            }
        }

        /// <summary>
        ///     Files </summary>
        public ObservableCollection<FileViewModel> Files { get; set; }

        #region [ Comments ]

        public string Comments {
            get { return _comments; }
            set {
                if (_comments != value) {
                    _comments = value;
                    if (String.IsNullOrEmpty(value) == false) {
                        try {
                            var doc = XDocument.Parse(value);
                            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                            namespaceManager.AddNamespace("t", "clr-namespace:Telerik.Windows.Documents.Model;assembly=Telerik.Windows.Documents");
                            _commentsSpans = doc.XPathSelectElements(@"t:RadDocument//t:Section//t:Paragraph//t:Span", namespaceManager).ToList();
                        } catch {
                            _commentsSpans = null;
                        }
                    } else {
                        _commentsSpans = null;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CommentsText));
                    OnPropertyChanged(nameof(CommentsForeground));
                    OnPropertyChanged(nameof(CommentsBackground));
                }
            }
        }

        // Extra
        private List<XElement> _commentsSpans;
        private bool doNotUse;
        private int? _totalLoads;

        // Extra
        public string CommentsText {
            get {
                try {
                    if (_commentsSpans != null && _commentsSpans.Any()) {
                        return String.Join(" ", _commentsSpans.Select(e => e.Attribute("Text").Value)).ReplaceAll("  ", " ");
                    }
                    return String.Empty;
                } catch {
                    return "???";
                }
            }
        }
        // Extra
        public SolidColorBrush CommentsForeground {
            get {
                if (_commentsSpans != null && _commentsSpans.Any()) {
                    try {
                        var hex = _commentsSpans.First().Attribute("ForeColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Black);
            }
        }
        // Extra
        public SolidColorBrush CommentsBackground {
            get {
                if (_commentsSpans != null && _commentsSpans.Any()) {
                    try {
                        var hex = _commentsSpans.First().Attribute("HighlightColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public int? TotalLoads {
            get => _totalLoads;
            set {
                if (_totalLoads != value) {
                    _totalLoads = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CarrierViewLink {
            get {
                if (this.DotNumber == null && this.McNumber == null) {
                    return null;
                }

                var urlParameter = String.Empty;
                if (this.DotNumber != null && this.McNumber != null) {
                    urlParameter = $"dot={this.DotNumber}&docket={this.McNumber}";
                } else if (this.DotNumber != null && this.McNumber == null) {
                    urlParameter = $"dot={this.DotNumber}";
                } else if (this.DotNumber == null && this.McNumber != null) {
                    urlParameter = $"docket={this.McNumber}";
                }
                return $"https://brokersnapshot.com/SearchCompanies/Search?{urlParameter}";
            }
        }

        #endregion [ Comments ]

        public DatCompanyViewModel() {
            this.Files = new ObservableCollection<FileViewModel>();
            this.Contacts = new ContactsViewModel();
        }

        public void Clear() {
            Loads.Clear();
        }

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (
                  (Name != null && Name.ToLower().ContainsWithoutSpace(search)) ||
                  (Contacts != null && Contacts.ToString().ToLower().Contains(search)) ||
                  (McNumber.ToString().ToLower().Contains(search)) ||
                  (DotNumber.ToString().ToLower().Contains(search)) ||
                  (Int32.TryParse(search.Trim(), out var searchMc) && searchMc == McNumber));
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            return result;
        }

        public void AssignSetupPacketProperties(DatCompanyViewModel data) {
            this.SetupPacketAccept = data.SetupPacketAccept;
            this.SetupPacketCompleted = data.SetupPacketCompleted;
            this.SetupPacketEmailSent = data.SetupPacketEmailSent;
            this.SetupPacketId = data.SetupPacketId;
            this.SetupPacketSent = data.SetupPacketSent;
            this.SetupPacketFile = data.SetupPacketFile;
            this.W9File = data.W9File;
        }

        /// <summary>
        ///     Checks current properties with other object by changes </summary>
        /// <returns>True: has changes; False: Equals</returns>
        public bool HasChanges(DatCompanyViewModel origin) {
            return !(
                this.Contacts.CompareTo(origin.Contacts) == 0 &&
                this.McNumber == origin.McNumber &&
                this.InsuranceExpirationChanged == false &&
                this.Comments == origin.Comments);
        }

        public override object Clone() {
            var clone = (DatCompanyViewModel)base.Clone();
            clone.Loads = new ObservableCollection<Wrapper1LoadViewModel>();
            clone.Loads.AddRange(this.Loads);

            clone.Contacts = (ContactsViewModel)this.Contacts.Clone();

            return clone;
        }
    }
}
