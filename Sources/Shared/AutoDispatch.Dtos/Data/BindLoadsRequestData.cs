using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of bind loads to trailer </summary>
    [Serializable]
    public class BindLoadsRequestData : BaseEventRequestData {

        public Guid? ParentId { get; set; }
        public LoadParentType ParentType { get; set; }
        public List<Guid> LoadIds { get; set; }

        public BindLoadsRequestData(Guid userId) : base(userId) {
        }

    }
}
