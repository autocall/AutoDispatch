using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileLoadRequestData : BaseEventRequestData {
        public FileDto File { get; set; }

        public FileLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
