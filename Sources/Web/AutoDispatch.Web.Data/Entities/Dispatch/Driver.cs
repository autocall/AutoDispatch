using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class Driver : BaseDispatchEntity {

    public Guid? TruckId { get; set; }

    public Guid? CurrentLoadId { get; set; }

    [Required]
    public byte DriverStatus { get; set; }

    public int? CarrierViewId { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? CarrierViewLastUpdatedDate { get; set; }

    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(256)]
    public string Email { get; set; }

    [MaxLength(64)]
    public string Phone { get; set; }

    [ForeignKey("TruckId")]
    public Truck Truck { get; set; }

    [ForeignKey("CurrentLoadId")]
    public Load Load { get; set; }

    #region [ Files ]

    [MaxLength(256)]
    public string THE_APPLICATION_OF_EMPLOYMENT_File { get; set; }
    [MaxLength(256)]
    public string MVR_File { get; set; }
    [MaxLength(256)]
    public string MEDICAL_CARD_File { get; set; }
    [MaxLength(256)]
    public string CDL_File { get; set; }
    [MaxLength(256)]
    public string DOT_INSPECTIONS_File { get; set; }
    [MaxLength(256)]
    public string BACKGROUND_File { get; set; }
    [MaxLength(256)]
    public string DRUG_TEST_File { get; set; }
    [MaxLength(256)]
    public string RANDOM_DRUG_TEST_File { get; set; }
    [MaxLength(256)]
    public string ACH_File { get; set; }
    [MaxLength(256)]
    public string W9_File { get; set; }

    [Column(TypeName = "date")]
    public DateTime? MVR_Expiration { get; set; }
    [Column(TypeName = "date")]
    public DateTime? MEDICAL_CARD_Expiration { get; set; }
    [Column(TypeName = "date")]
    public DateTime? CDL_Expiration { get; set; }
    [Column(TypeName = "date")]
    public DateTime? ACH_Expiration { get; set; }
    [Column(TypeName = "date")]
    public DateTime? W9_Expiration { get; set; }

    #endregion [ Files ]

    /// <summary>
    ///     Грузы, где водитель — HistoryDriver </summary>
    [InverseProperty("HistoryDriver")]
    public ICollection<Load> Histories { get; set; }
}
