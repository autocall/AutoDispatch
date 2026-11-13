using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Carrier </summary>
    [DataContract]
    public class DatCompanyDto : BaseObjectDto, IBaseEventDto {

        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public Guid? ActualCompanyId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? McNumber { get; set; }

        [DataMember]
        public int? DotNumber { get; set; }

        [DataMember]
        public DatCompanyType EntityType { get; set; }

        [DataMember]
        public string Contacts { get; set; }

        [DataMember]
        public bool Phy_UpsStore { get; set; }
        [DataMember]
        public bool Mai_UpsStore { get; set; }
        [DataMember]
        public bool Phy_MailBox { get; set; }
        [DataMember]
        public bool Mai_MailBox { get; set; }

        [DataMember]
        public DateTime? InsuranceExpiration { get; set; }

        [DataMember]
        public string InsuranceFilePath { get; set; }

        [DataMember]
        public byte[] InsuranceFileData { get; set; }

        [DataMember]
        public string Comments { get; set; }

        [DataMember]
        public byte Rating { get; set; }

        [DataMember]
        public string HyperlinkFile { get; set; }

        #region [ setup packet ]

        [DataMember]
        public Guid? SetupPacketId { get; set; }

        [DataMember]
        public List<string> SetupPacketEmailSent { get; set; }

        [DataMember]
        public bool SetupPacketSent { get; set; }

        [DataMember]
        public bool SetupPacketCompleted { get; set; }

        [DataMember]
        public bool SetupPacketAccept { get; set; }

        [DataMember]
        public string SetupPacketFile { get; set; }

        [DataMember]
        public string W9File { get; set; }

        [DataMember]
        public bool DoNotUse { get; set; }

        #endregion [ setup packet ]

        /// <summary>
        ///     [Label, Field] from HTML </summary>
        [DataMember]
        public Dictionary<string, string> Fields { get; set; }
    }
}
