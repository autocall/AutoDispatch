using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveFactoringCompanyResponseData : BaseEventResponseData {
        [TargetResult]
        public string FactoringCompanyJson { get; set; }
    }
}
