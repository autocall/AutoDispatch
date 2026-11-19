using System;
using System.Collections.Generic;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SearchLoadsRequest : BaseRequest {
        public Dictionary<string, string> SearchTexts { get; set; }
    }
}
