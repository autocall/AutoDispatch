using AutoCall.Common.Attributes;
using AutoDispatch.Logic.Attributes;
using System.ComponentModel;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses {
    public enum ExpenseType {
        [Display(true, order: 0)]
        [DescriptionShort("")]
        [Description("")]
        none = 0,
        [Display(true, order: 1)]
        [DescriptionShort("pick")]
        [Description("Pickup")]
        pick = 1,
        [Display(true, order: 2)]
        [DescriptionShort("del")]
        [Description("Delivery")]
        del = 2,
        [Display(true, order: 3)]
        [DescriptionShort("whs")]
        [Description("Warehouse")]
        whs = 3,
        [Display(true, order: 4)]
        [DescriptionShort("in")]
        [Description("Ingate")]
        @in = 4,
        [Display(true, order: 5)]
        [DescriptionShort("out")]
        [Description("Outgate")]
        @out = 5,
        [Display(true, order: 6)]
        [DescriptionShort("late del")]
        [Description("Late Delivery")]
        lateDelivery = 6,
        [Display(true, order: 7)]
        [DescriptionShort("late pic")]
        [Description("Pick Up Late")]
        pickupLate = 7,
        [Display(true, order: 9)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other1 = 8,
        [Display(true, order: 10)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other2 = 9,
        [Display(true, order: 11)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other3 = 10,
        [Display(true, order: 12)]
        [DescriptionShort("other")]
        [Description("Other")]
        Other4 = 11,
    }
}