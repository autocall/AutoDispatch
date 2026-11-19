using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SearchByMcDotNumberRequest: BaseRequest {
        public int? DOT { get; set; }
        public int? MC { get; set; }
    }
}
