using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response available modules </summary>
    [Serializable]
    public class GetModulesResponseData : BaseEventResponseData {
        [TargetResult]
        public List<ModuleDto> Modules { get; set; }
    }
}
