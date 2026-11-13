using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveLoadsRequestData : BaseEventRequestData {
        public List<LoadDto> Loads { get; set; }

        public SaveLoadsRequestData(Guid userId) : base(userId) {
        }
    }
}
