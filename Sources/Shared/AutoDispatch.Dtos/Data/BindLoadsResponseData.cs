using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of loads </summary>
    [Serializable]
    public class BindLoadsResponseData : BaseEventResponseData {
        /// <summary>
        ///     Loads </summary>
        [TargetResult]
        public List<LoadDto> Loads { get; set; }
    }
}
