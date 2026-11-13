using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(Number))]
public class Trailer : BaseDispatchEntity {

    [MaxLength(32), Required]
    public string Number { get; set; }

    public byte Type { get; set; }

    #region [ Files ]

    [MaxLength(256)]
    public string AnnualInspection_File { get; set; }

    [Column(TypeName = "date")]
    public DateTime? AnnualInspection_Expiration { get; set; }

    [MaxLength(256)]
    public string Registration_File { get; set; }

    [MaxLength(256)]
    public string Maintenance_File { get; set; }

    [Column(TypeName = "date")]
    public DateTime? Maintenance_Expiration { get; set; }

    [MaxLength(256)]
    public string SchemeFile { get; set; }

    [MaxLength(256)]
    public string OriginSchemeFile { get; set; }

    [Column(TypeName = "date")]
    public DateTime? SchemeDate { get; set; }

    #endregion [ Files ]

    #region [ BNSF ]

    [MaxLength(128)]
    public string BnsfOwner { get; set; }
    [MaxLength(128)]
    public string BnsfShipper { get; set; }
    [MaxLength(128)]
    public string BnsfUnit { get; set; }
    [MaxLength(128)]
    public string BnsfLoadEmptyStatus { get; set; }
    [MaxLength(128)]
    public string BnsfCurrentLocation { get; set; }
    [MaxLength(128)]
    public string BnsfEventCode { get; set; }
    [MaxLength(128)]
    public string BnsfEventDate { get; set; }
    [MaxLength(128)]
    public string BnsfETA_Deramp { get; set; }
    [MaxLength(128)]
    public string BnsfOrigin { get; set; }
    [MaxLength(128)]
    public string BnsfDestination { get; set; }
    [MaxLength(128)]
    public string BnsfBeyondGoalHours { get; set; }
    [MaxLength(128)]
    public string BnsfTimeDeramp { get; set; }
    [MaxLength(128)]
    public string BnsfTimeNotify { get; set; }
    [MaxLength(128)]
    public string BnsfFinalTrainArrival { get; set; }
    [MaxLength(128)]
    public string BnsfGoalDate { get; set; }
    [MaxLength(128)]
    public string BnsfNextevent { get; set; }
    [MaxLength(128)]
    public string BnsfPickUpNumber { get; set; }
    [MaxLength(128)]
    public string BnsfStorageDay { get; set; }
    [MaxLength(128)]
    public string BnsfTrain { get; set; }
    [MaxLength(128)]
    public string BnsfTrucker { get; set; }
    [MaxLength(128)]
    public string BnsfWaybillDate { get; set; }
    [MaxLength(128)]
    public string BnsfRateAuthority { get; set; }
    [MaxLength(128)]
    public string BnsfAmountDue { get; set; }
    [MaxLength(128)]
    public string BnsfDueDate { get; set; }
    [MaxLength(128)]
    public string BnsfWaybillNumber { get; set; }
    [MaxLength(128)]
    public string BnsfBillNumber { get; set; }
    [MaxLength(128)]
    public string BnsfAdjustments { get; set; }

    #endregion [ BNSF ]

    public bool History { get; set; }

    public Guid? YardId { get; set; }

    [ForeignKey("YardId")]
    public Yard Yard { get; set; }

    [InverseProperty("Trailer")]
    public ICollection<Load> Loads { get; set; }
}
