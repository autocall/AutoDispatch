namespace AutoDispatch.Logic.DataServices.Models {
    public class ExceptionModel {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string MessageDetail { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
    }
}
