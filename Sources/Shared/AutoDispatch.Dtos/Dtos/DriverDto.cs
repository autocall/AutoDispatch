using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class DriverDto : BaseDispatchDto, IBaseEventDto {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public ChatStatus ChatStatus { get; set; }

        [DataMember]
        public Guid? TruckId { get; set; }

        [DataMember]
        public Guid? LoadId { get; set; }

        [DataMember]
        public DriverStatus DriverStatus { get; set; }

        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public int? CarrierViewId { get; set; }
        [DataMember]
        public DateTime? CarrierViewLastUpdatedDate { get; set; }

        #region [ Files ]

        [DataMember]
        public string THE_APPLICATION_OF_EMPLOYMENT_File { get; set; }
        [DataMember]
        public string MVR_File { get; set; }
        [DataMember]
        public string MEDICAL_CARD_File { get; set; }
        [DataMember]
        public string CDL_File { get; set; }
        [DataMember]
        public string DOT_INSPECTIONS_File { get; set; }
        [DataMember]
        public string BACKGROUND_File { get; set; }
        [DataMember]
        public string DRUG_TEST_File { get; set; }
        [DataMember]
        public string RANDOM_DRUG_TEST_File { get; set; }
        [DataMember]
        public string ACH_File { get; set; }
        [DataMember]
        public string W9_File { get; set; }

        [DataMember]
        public DateTime? MVR_Expiration { get; set; }
        [DataMember]
        public DateTime? MEDICAL_CARD_Expiration { get; set; }
        [DataMember]
        public DateTime? CDL_Expiration { get; set; }
        [DataMember]
        public DateTime? ACH_Expiration { get; set; }
        [DataMember]
        public DateTime? W9_Expiration { get; set; }

        #endregion [ Files ]
    }
}
