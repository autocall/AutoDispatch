using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of drivers </summary>
    [Serializable]
    public class LoadDriversResponseData : BaseEventResponseData {
        /// <summary>
        ///     Drivers </summary>
        [TargetResult]
        public List<DriverDto> Drivers { get; set; }
    }
}
