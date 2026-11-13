using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class ActualCompanyDto : BaseDto {

        [DataMember]
        public string LogoSmall { get; set; }
        [DataMember]
        public string LogoBig { get; set; }

        [DataMember]
        public int McNumber { get; set; }

        [DataMember]
        public Guid CompanyId { get; set; }

        [DataMember]
        public string LegalName { get; set; }

        [DataMember]
        public string PhysicalAddress { get; set; }
        [DataMember]
        public string MailingAddress { get; set; }

        [DataMember]
        public string ShippersPhone { get; set; }
        [DataMember]
        public string CellPhone { get; set; }

        [DataMember]
        public string WorkPhone { get; set; }
        [DataMember]
        public string WorkFax { get; set; }
        [DataMember]
        public string WorkEmail { get; set; }

        [DataMember]
        public string PaymentInstructions { get; set; }

        [DataMember]
        public string SMTP_Email { get; set; }
        [DataMember]
        public string SMTP_Password { get; set; }
        [DataMember]
        public string SMTP_Host { get; set; }
        [DataMember]
        public int SMTP_Port { get; set; }

        [DataMember]
        public string SMTP_SchemeEmail { get; set; }
        [DataMember]
        public string SMTP_SchemePassword { get; set; }
        [DataMember]
        public string SMTP_SchemeHost { get; set; }
        [DataMember]
        public int SMTP_SchemePort { get; set; }

        [DataMember]
        public string SMS_Phone { get; set; }

        [DataMember]
        public string ConfirmationFile { get; set; }
        [DataMember]
        public string BOLFile { get; set; }
        [DataMember]
        public string InvoiceFile { get; set; }

        [DataMember]
        public string ConfirmationData { get; set; }
        [DataMember]
        public string BOLData { get; set; }
        [DataMember]
        public string InvoiceData { get; set; }

        [DataMember]
        public DatCompanyType EntityType { get; set; }
        [DataMember]
        public DispatchBoardType BoardType { get; set; }

        [DataMember]
        public int NextConfirmationNumber { get; set; }
        [DataMember]
        public int NextInvoiceNumber { get; set; }
        [DataMember]
        public int NextLoadNumber { get; set; }
        [DataMember]
        public byte[] NextLoadExtra { get; set; }
        [DataMember]
        public bool InvoiceNumberAsLoadNumber { get; set; }

        [DataMember]
        public string BnsfLogin { get; set; }
        [DataMember]
        public string BnsfPassword { get; set; }
        [DataMember]
        public string BnsfCookies { get; set; }
        [DataMember]
        public string BnsfLastMessage { get; set; }
        [DataMember]
        public DateTime? BnsfLastDone { get; set; }

        [DataMember]
        public bool SubmitRates { get; set; }

        [DataMember]
        public string CarrierViewToken { get; set; }

        [DataMember]
        public string FactoringCompanyJson { get; set; }

        [DataMember]
        public List<ActualCompanyCustomDto> Customizes { get; set; }
    }
}
