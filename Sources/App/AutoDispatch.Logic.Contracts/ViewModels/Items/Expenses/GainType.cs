using AutoCall.Common.Attributes;
using AutoDispatch.Logic.Attributes;
using System.ComponentModel;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses {
    public enum GainType {
        [Display(true, order: 0)]
        [DescriptionShort("")]
        [Description("")]
        none = 0,
        [Display(true, order: 1)]
        [DescriptionShort("lum")]
        [Description("Lumper")]
        Lumper,
        [Display(true, order: 2)]
        [DescriptionShort("det")]
        [Description("Detention")]
        detention,
        [Display(true, order: 3)]
        [DescriptionShort("dr assist")]
        [Description("Driver Assist")]
        driverAssist,
        [Display(true, order: 4)]
        [DescriptionShort("drop tra")]
        [Description("Drop Trailer")]
        dropTrailer,
        [Display(true, order: 5)]
        [DescriptionShort("redel")]
        [Description("Redelivery")]
        Redelivery,
        [Display(true, order: 12)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other1,
        [Display(true, order: 13)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other2,
        [Display(true, order: 14)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other3,
        [Display(true, order: 15)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other4,
        [Display(true, order: 7)]
        [DescriptionShort("chassis")]
        [Description("Chassis")]
        Chassis,
        [Display(true, order: 8)]
        [DescriptionShort("over wght")]
        [Description("Overweight")]
        Overweight,
        [Display(true, order: 9)]
        [DescriptionShort("warehouse srv")]
        [Description("Warehouse Services")]
        WarehouseServices,
        [Display(true, order: 10)]
        [DescriptionShort("drayage")]
        [Description("Drayage")]
        Drayage,
        [Calculable]
        [Display(true, order: 11)]
        [DescriptionShort("-12% srv")]
        [Description("-12% LR For Service")]
        LRForService12,
    }
}