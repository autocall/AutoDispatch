using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadLoadsZipResponseData : BaseEventResponseData {
        [TargetResult]
        public List<byte[]> LoadsZip { get; set; }
    }
}
