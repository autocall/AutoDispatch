using AutoCall.Common.Attributes;
using System.ComponentModel;

namespace AutoDispatch.Logic.Contracts.Enums {
    public enum FilterColumnEnum {
        [Description("Equipment"), Display(true, 4)]
        Equipment = 0,
        [Description("Status"), Display(true, 5)]
        Status = 1,
        [Description("CustomStatus"), Display(true, 6)]
        CustomStatus = 2,
        [Description("Origin"), Display(true, 0)]
        Origin = 3,
        [Description("Destination"), Display(true, 1)]
        Destination = 4,
        [Description("Driver"), Display(true, 7)]
        Driver = 5,
        [Description("Truck"), Display(true, 8)]
        Truck = 6,
        [Description("Trailer"), Display(true, 9)]
        Trailer = 7,
        [Description("Warehouse"), Display(true, 10)]
        Warehouse = 8,
        [Description("Pickup"), Display(true, 2)]
        Pickup = 9,
        [Description("Delivery"), Display(true, 3)]
        Delivery = 10,
        [Description("Load#"), Display(true, 12)]
        Index = 11,
        [Description("Created"), Display(true, 2)]
        Created = 12,
        [Description("Created By"), Display(true, 13)]
        CreatedByUser = 13,
        [Description("Destination Warehouse"), Display(true, 11)]
        DestinationWarehouse = 14,
    }
}
