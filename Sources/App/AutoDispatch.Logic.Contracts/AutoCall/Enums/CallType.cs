using System.ComponentModel;

namespace AutoCall.Logic.Enums {
    public enum CallType {
        [Description("Single Call")]
        SingleCall,
        [Description("Multi Call")]
        MultiCall,
        [Description("Rich Call")]
        RichCall,
        //[Description("Saturated Call")]
        //SaturatedCall,
    }
}
