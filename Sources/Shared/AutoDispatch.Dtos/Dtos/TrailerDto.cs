using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class TrailerDto : BaseDispatchDto, IBaseEventDto {

        [DataMember] public string Number { get; set; }
        [DataMember] public byte Type { get; set; }

        [DataMember] public int EventId { get; set; }

        #region [ Files ]

        [DataMember] public string AnnualInspection_File { get; set; }
        [DataMember] public DateTime? AnnualInspection_Expiration { get; set; }
        [DataMember] public string Registration_File { get; set; }
        [DataMember] public string Maintenance_File { get; set; }
        [DataMember] public DateTime? Maintenance_Expiration { get; set; }

        [DataMember] public DateTime? SchemeDate { get; set; }
        [DataMember] public string SchemeFile { get; set; }
        [DataMember] public string OriginSchemeFile { get; set; }

        #endregion [ Files ]

        #region [ BNSF ]

        [DataMember] public string BnsfOwner { get; set; }
        [DataMember] public string BnsfShipper { get; set; }
        [DataMember] public string BnsfUnit { get; set; }
        [DataMember] public string BnsfLoadEmptyStatus { get; set; }
        [DataMember] public string BnsfCurrentLocation { get; set; }
        [DataMember] public string BnsfEventCode { get; set; }
        [DataMember] public string BnsfEventDate { get; set; }
        [DataMember] public string BnsfETA_Deramp { get; set; }
        [DataMember] public string BnsfOrigin { get; set; }
        [DataMember] public string BnsfDestination { get; set; }
        [DataMember] public string BnsfBeyondGoalHours { get; set; }
        [DataMember] public string BnsfTimeDeramp { get; set; }
        [DataMember] public string BnsfTimeNotify { get; set; }
        [DataMember] public string BnsfFinalTrainArrival { get; set; }
        [DataMember] public string BnsfGoalDate { get; set; }
        [DataMember] public string BnsfNextevent { get; set; }
        [DataMember] public string BnsfPickUpNumber { get; set; }
        [DataMember] public string BnsfStorageDay { get; set; }
        [DataMember] public string BnsfTrain { get; set; }
        [DataMember] public string BnsfTrucker { get; set; }
        [DataMember] public string BnsfWaybillDate { get; set; }
        [DataMember] public string BnsfRateAuthority { get; set; }

        [DataMember] public string BnsfAmountDue { get; set; }
        [DataMember] public string BnsfDueDate { get; set; }
        [DataMember] public string BnsfWaybillNumber { get; set; }
        [DataMember] public string BnsfBillNumber { get; set; }
        [DataMember] public string BnsfAdjustments { get; set; }

        #endregion [ BNSF ]

        [DataMember] public bool History { get; set; }

        [DataMember] public Guid? YardId { get; set; }

        [DataMember] public List<DefectDto> Defects { get; set; }
    }
}
