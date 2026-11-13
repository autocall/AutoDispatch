
using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileLoadResponseData : BaseEventResponseData {
        [TargetResult]
        public FileDto File { get; set; }
    }
}
