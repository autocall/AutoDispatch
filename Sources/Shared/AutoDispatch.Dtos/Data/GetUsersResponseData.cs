using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response users of chat </summary>
    [Serializable]
    public class GetUsersResponseData : BaseEventResponseData {
        /// <summary>
        ///     List of users </summary>
        [TargetResult]
        public List<UserDispatchDto> Users {get; set; }
    }
}
