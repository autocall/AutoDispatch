using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FileRenameRequestData : BaseEventRequestData {

        public FileDto File { get; set; }

        public string NewName { get; set; }

        public FileRenameRequestData(Guid userId) : base(userId) {
        }
    }
}
