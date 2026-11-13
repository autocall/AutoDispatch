using AutoCall.Common.Attributes;
using System.ComponentModel;

namespace AutoDispatch.Dtos.Enums {
    public enum LoadStatus {
        [Display(true, order: 0)]
        [Description("none")]
        None = 0,
        [Display(true, order: 1)]
        [Description("dispatched")]
        Dispatched = 16,
        [Display(true, order: 2)]
        [Description("assigned")]
        Assigned = 17,
        [Display(true, order: 3)]
        [Description("at the pick up")]
        AtPickUp = 1,
        [Display(false, order: 4)]
        [Description("waiting")]
        Waiting1 = 9,
        //не используемый
        [Display(true, order: 5)]
        [Description("picked up")]
        PickedUp = 10,
        [Display(true, order: 6)]
        [Description("in transit")]
        InTransit = 2,
        [Display(true, order: 7)]
        [Description("at delivery")]
        AtDelivery = 11,
        [Display(false, order: 8)]
        [Description("waiting")]
        Waiting2 = 12,
        [Display(true, order: 9)]
        [Description("delivered")]
        Delivered = 3,
        [Display(true, order: 10)]
        [Description("billed")]
        Billed = 4,
        [Display(true, order: 11)]
        [Description("issue")]
        Issues = 13,
        [Display(true, order: 12)]
        [Description("bond")]
        Bond = 15,
        [Display(false, order: 13)]
        // Dispatch -> Accounting
        [Description("canceled1")]
        Canceled1 = 7,
        // Accounting -> History
        [Display(false, order: 14)]
        [Description("canceled2")]
        Canceled2 = 8,
        [Display(false, order: 15)]
        [Description("unpaid")]
        Unpaid = 14,
        [Display(false, order: 16)]
        [Description("paid")]
        Completed = 6,
    }
}
