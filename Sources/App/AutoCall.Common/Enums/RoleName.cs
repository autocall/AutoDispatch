using AutoCall.Common.Attributes;
using System;
using System.ComponentModel;

namespace AutoCall.Common.Enums {
    /// <summary>
    ///     All possible roles of user </summary>
    [Flags]
    public enum RoleName {
        [Description("User")/*Load Board*/, Display(true, order: 0)]
        User = 0x02,
        [Description("Driver"), Display(true, order: 1)]
        Driver = 0x08,
        [Description("Warehouse"), Display(true, order: 4)]
        Warehouse = 0x10,
        [Description("Dispatcher"), Display(true, order: 5)]
        Dispatcher = 0x04,
        [Description("Accounting"), Display(true, order: 7)]
        Accounting = 0x20,
        [Description("Owner"), Display(true, order: 8)]
        Owner = 0x40,
        [Description("Admin"), Display(true, order: 9)]
        Admin = 0x01,
        [Description("Sales"), Display(true, order: 6)]
        Sales = 0x80,
        [Description("User-Agent"), Display(true, order: 3)]
        Agent = 0x100,
        [Description("Gps Support"), Display(true, order: 2)]
        GpsSupport = 0x200,
    }
}