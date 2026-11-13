using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    public class BnsfRailwayTrailerDto {
        [DataMember] public string Owner { get; set; }
        [DataMember] public string Shipper { get; set; }
        [DataMember] public string Unit { get; set; }
        [DataMember] public string LoadEmptyStatus { get; set; }
        [DataMember] public string CurrentLocation { get; set; }
        [DataMember] public float Latitude { get; set; }
        [DataMember] public float Longitude { get; set; }
        [DataMember] public string EventCode { get; set; }
        [DataMember] public string EventDate { get; set; }
        [DataMember] public string ETA_Deramp { get; set; }
        [DataMember] public string Origin { get; set; }
        [DataMember] public string Destination { get; set; }
        [DataMember] public string BeyondGoalHours { get; set; }
        [DataMember] public string TimeDeramp { get; set; }
        [DataMember] public string TimeNotify { get; set; }
        [DataMember] public string FinalTrainArrival { get; set; }
        [DataMember] public string GoalDate { get; set; }
        [DataMember] public string Nextevent { get; set; }
        [DataMember] public string PickUpNumber { get; set; }
        [DataMember] public string StorageDay { get; set; }
        [DataMember] public string Train { get; set; }
        [DataMember] public string Trucker { get; set; }
        [DataMember] public string WaybillDate { get; set; }
        [DataMember] public string RateAuthority { get; set; }

        [DataMember] public string AmountDue { get; set; }
        [DataMember] public string DueDate { get; set; }
        [DataMember] public string WaybillNumber { get; set; }
        [DataMember] public string BillNumber { get; set; }
        [DataMember] public string Adjustments { get; set; }
    }
}
