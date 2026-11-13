
using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileSaveResponseData : BaseEventResponseData {
        [TargetResult]
        public FileDto File { get; set; }
    }
}
