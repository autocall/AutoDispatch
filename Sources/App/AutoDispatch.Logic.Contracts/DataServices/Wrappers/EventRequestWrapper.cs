using System;
using System.Threading.Tasks;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.Services;

namespace AutoDispatch.Logic.DataServices.Wrappers {

    /// <summary>
    ///     Represents base request wrapper </summary>
    /// <remarks>Wrapper above request</remarks>
    public abstract class EventRequestWrapper {
        /// <summary>
        ///     Data of event </summary>
        public EventRequest Request { get; }


        /// <summary>
        ///     .ctor </summary>
        /// <param name="request">Event Request</param>
        public EventRequestWrapper(EventRequest request) {
            Request = request;
        }

        /// <summary>
        ///     Invoke logic complete delegat </summary>
        public abstract Task InvokeCompletedAsync(EventResponse response);
    }

    /// <summary>
    ///     Represents generic request wrapper </summary>
    /// <remarks>
    /// Wrapper above request
    /// Allow sets completed delegate
    /// </remarks>
    public class EventRequestWrapper<TResult> : EventRequestWrapper {

        /// <summary>
        ///     Delegate from logic layer </summary>
        private Func<EventResponse, Func<ViewModelWrapper<TResult>, Task>, Task> LogicCompletedAsync { get; }
        /// <summary>
        ///     Delegate from view layer </summary>
        private Func<ViewModelWrapper<TResult>, Task> ViewCompletedAsync { get; }

        /// <summary>
        ///     .ctor </summary>
        /// <param name="request">Event Request</param>
        /// <param name="logicCompleted">Logic completed delegate</param>
        /// <param name="viewCompleted">View completed delegate</param>
        public EventRequestWrapper(
            EventRequest request,
            Func<EventResponse, Func<ViewModelWrapper<TResult>, Task>, Task> logicCompleted,
            Func<ViewModelWrapper<TResult>, Task> viewCompleted)
            : base(request) {
            LogicCompletedAsync = logicCompleted;
            ViewCompletedAsync = viewCompleted;
        }

        /// <summary>
        ///     Invoke logic complete delegate </summary>
        public override async Task InvokeCompletedAsync(EventResponse response) {
            await LogicCompletedAsync.Invoke(response, ViewCompletedAsync);
        }
    }

    /// <summary>
    ///     Represents generic request wrapper </summary>
    /// <remarks>
    /// Wrapper above request
    /// Allow sets completed delegate
    /// </remarks>
    public class EventRequestWrapper<TResult, TEventResponseData> : EventRequestWrapper where TEventResponseData : BaseEventResponseData {

        /// <summary>
        ///     Delegate from logic layer </summary>
        private Func<EventResponse, Func<ViewModelWrapper<TResult>, Task>, Func<TEventResponseData, TResult>, Task> LogicCompleted { get; }
        /// <summary>
        ///     Delegate from view layer </summary>
        private Func<ViewModelWrapper<TResult>, Task> ViewCompleted { get; }
        /// <summary>
        ///     Specify comverter DTO => ViewMdoel
        /// </summary>
        private Func<TEventResponseData, TResult> Converter { get; }

        /// <summary>
        ///     .ctor </summary>
        /// <param name="request">Event Request</param>
        /// <param name="logicCompleted">Logic completed delegate</param>
        /// <param name="viewCompleted">View completed delegate</param>
        /// <param name="converter">Response to ViewModel converter</param>
        public EventRequestWrapper(
            EventRequest request,
            Func<EventResponse, Func<ViewModelWrapper<TResult>, Task>, Func<TEventResponseData, TResult>, Task> logicCompleted,
            Func<ViewModelWrapper<TResult>, Task> viewCompleted,
            Func<TEventResponseData, TResult> converter = null)
            : base(request) {
            LogicCompleted = logicCompleted;
            ViewCompleted = viewCompleted;
            Converter = converter;
        }

        /// <summary>
        ///     Invoke logic complete delegate </summary>
        public override async Task InvokeCompletedAsync(EventResponse response) {
            await LogicCompleted.Invoke(response, ViewCompleted, Converter);
        }
    }
}
