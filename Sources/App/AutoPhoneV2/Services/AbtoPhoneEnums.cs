using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Logic.Services {

    public enum abtophone_state {
        Ringing = 0,
        Connecting = 1,
        Connected = 2,
        Failed = 3,
        Ended = 4,
        Hold = 5,
        SET_ACCOUNT = 6,
        DETAILS = 7,
        Transferred= 8,
    }

    public enum abtophone_action {
        MAKE_CALL,
        HANGUP,
        HANGUP_ALL,
        HOLD,
        RESUME,
        TRANSFER,
        ACTION_DTMF,
        SET_ACCOUNT
    }

    public class AbtoPhoneAction {
        public string header { get; set; }
        public string subHeader { get; set; }
        public string loadId { get; set; }
        public int action { get; set; }
        public string phone { get; set; }
    }

    public class AbtoPhoneState {
        public int state { get; set; }
        // example: 8001231234
        public string phone { get; set; }
        /// <summary>
        ///     1 .. 4 </summary>
        public int? line { get; set; }
    }

    // IAutoPhoneV2CallService V1 убран — конкретный AutoPhoneV2CallService (DI без интерфейсов, AI/wiki/plan.md)
}
