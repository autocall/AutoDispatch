using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Logic.Dto;
using AutoCall.Logic.Enums;

namespace AutoCall.Logic.Services {
    public class NoneCallService : CallService, ICallService, IDisposable {
        public override CallServiceType CallServiceType => CallServiceType.Unknown;
    }
}