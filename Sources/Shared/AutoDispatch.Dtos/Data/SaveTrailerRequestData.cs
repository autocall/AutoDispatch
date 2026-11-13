using AutoDispatch.Dtos.Dtos;
using Newtonsoft.Json;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveTrailerRequestData : BaseEventRequestData {
        public TrailerDto Trailer { get; set; }
        public bool SkipDefects { get; set; }

        public SaveTrailerRequestData(Guid userId) : base(userId) {
        }
    }
}
