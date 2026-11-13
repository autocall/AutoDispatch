using System.ComponentModel;
using AutoCall.Common.Extensions;

namespace AutoDispatch.Web.Logic.Dtos.Dispatch;

// Expenses/Gains груза хранятся в Load.Expenses/Gains как JSON view-моделей клиента
// (AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses, $type с именем сборки клиента). Сервер читает их для писем
// и ставки; типы повторяют клиентские (V1 Converters/Dispatch/Expenses), без атрибутов DescriptionShort/Calculable

public enum ExpenseType {
    [Description("")] none = 0,
    [Description("Pickup")] pick = 1,
    [Description("Delivery")] del = 2,
    [Description("Warehouse")] whs = 3,
    [Description("Ingate")] @in = 4,
    [Description("Outgate")] @out = 5,
    [Description("Late Delivery")] lateDelivery = 6,
    [Description("Pick Up Late")] pickupLate = 7,
    [Description("Other")] Other1 = 8,
    [Description("Other")] Other2 = 9,
    [Description("Other")] Other3 = 10,
    [Description("Other")] Other4 = 11,
}

public enum GainType {
    [Description("")] none = 0,
    [Description("Lumper")] Lumper,
    [Description("Detention")] detention,
    [Description("Driver Assist")] driverAssist,
    [Description("Drop Trailer")] dropTrailer,
    [Description("Redelivery")] Redelivery,
    [Description("Other")] Other1,
    [Description("Other")] Other2,
    [Description("Other")] Other3,
    [Description("Other")] Other4,
    [Description("Chassis")] Chassis,
    [Description("Overweight")] Overweight,
    [Description("Warehouse Services")] WarehouseServices,
    [Description("Drayage")] Drayage,
    [Description("-12% LR For Service")] LRForService12,
}

public enum WeightType : byte {
    [Description("kLbs")] kLbs,
    [Description("Lbs")] Lbs
}

public class ExpenseViewModel {
    public decimal? Cost { get; set; }
    public string Comment { get; set; }
    public bool Paid { get; set; }
    public DateTime? PaidDate { get; set; }
    public ExpenseType Type { get; set; }
}

public class GainViewModel {
    public decimal? Cost { get; set; }
    public string Comment { get; set; }
    public bool Paid { get; set; }
    public DateTime? PaidDate { get; set; }
    public GainType Type { get; set; }

    public string TypeDescription() {
        if (new[] { GainType.Other1, GainType.Other2, GainType.Other3, GainType.Other4 }.Contains(this.Type) &&
            String.IsNullOrEmpty(this.Comment) == false) {
            return this.Comment;
        }
        return this.Type.GetDescription();
    }
}
