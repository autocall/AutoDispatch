using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels;

namespace AutoDispatch.Logic.Services {
    public class ViewModelWrapper<T> {
        public ResponseStatus Status { get; set; }
        public string MessageError { get; set; }
        public T Data { get; set; }

        public ViewModelWrapper(T data) {
            Data = data;
            Status = ResponseStatus.Success;
        }

        public ViewModelWrapper(string messageError) {
            MessageError = messageError;
            Status = ResponseStatus.Failed;
        }
    }
}
