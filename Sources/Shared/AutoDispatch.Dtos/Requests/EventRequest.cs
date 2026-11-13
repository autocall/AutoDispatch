using System;
using AutoDispatch.Dtos.Data;

namespace AutoDispatch.Dtos.Requests {
    [Serializable]
    public class EventRequest {
        /// <summary>
        ///     Name of Request, Response and Method name </summary>
        public string Name { get; set; }
        /// <summary>
        ///     Base Data of request </summary>
        public BaseEventRequestData Data { get; set; }
    }
}
