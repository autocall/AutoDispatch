
using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FilesLoadResponseData : BaseEventResponseData {
        [TargetResult]
        public List<FileDto> Files { get; set; }
    }
}
