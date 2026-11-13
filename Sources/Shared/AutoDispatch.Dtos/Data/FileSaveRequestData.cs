using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileSaveRequestData : BaseEventRequestData {

        public FileDto File { get; set; }

        public FileSaveRequestData(Guid userId) : base(userId) {
        }
    }
}
