using System;

namespace AutoDispatch.Logic.Managers.Results {
    /// <summary>
    ///     Represents base result from main manager </summary>
    public class BaseResult {
        public BaseResult() {
            Successed = true;
        }

        /// <summary>
        ///     Convert Exception to message (ErrorMessage) for BaseResult. </summary>
        public void ErrorResult(Exception exception) {
            Successed = false;
            Exception = exception.GetBaseException();
        }

        /// <summary>
        ///     Gets or sets is successed </summary>
        public bool Successed { get; private set; }

        /// <summary>
        ///     Gets or sets exception </summary>
        public Exception Exception { get; private set; }
    }
}