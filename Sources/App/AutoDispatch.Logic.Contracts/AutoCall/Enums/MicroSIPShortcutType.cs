using AutoCall.Common.Attributes;
using System.ComponentModel;

namespace AutoCall.Logic.Enums {
    public enum MicroSIPShortcutType {
        [Description("Call"), Display(true, 0)]
        MSIP_SHORTCUT_CALL,
        [Description("Video Call"), Display(false)]
        MSIP_SHORTCUT_VIDEOCALL,
        [Description("Message"), Display(false)]
        MSIP_SHORTCUT_MESSAGE,
        [Description("DTMF"), Display(false)]
        MSIP_SHORTCUT_DTMF,
        [Description("Transfer"), Display(true, 1)]
        MSIP_SHORTCUT_TRANSFER
    }
}
