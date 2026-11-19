using System;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SearchCustomersRequest : BaseRequest {
        public string Text { get; set; }
    }
}
