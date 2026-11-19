using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     Represents base response for data services </summary>
    public class BaseResponse {
        /// <summary>
        ///     Gets or sets response status </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets error message </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets is response successed </summary>
        public bool IsSuccessed {
            get { return Status == ResponseStatus.Success; }
        }

        /// <summary>
        ///     Constructor </summary>
        public BaseResponse() {
            Status = ResponseStatus.Success;
        }
    }
}