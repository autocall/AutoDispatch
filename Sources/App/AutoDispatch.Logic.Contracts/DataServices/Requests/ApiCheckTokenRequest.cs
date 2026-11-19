namespace AutoDispatch.Logic.DataServices.Requests {
    /// <summary>
    ///     Represents AutoCall API request to get login result </summary>
    public class ApiCheckTokenRequest : BaseRequest {
        public string Token { get; set; }
    }
}