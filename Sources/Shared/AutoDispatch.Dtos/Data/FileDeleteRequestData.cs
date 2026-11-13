using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileDeleteRequestData : BaseEventRequestData {
        public FileDto File { get; set; }

        public FileDeleteRequestData(Guid userId) : base(userId) {
        }
    }
}
