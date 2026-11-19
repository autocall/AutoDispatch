using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Common.Loggers;
using AutoDispatch.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Wrappers;
using AutoDispatch.Logic.Services;

namespace AutoDispatch.Logic.Managers {
    public class EngineManager {

        #region [ _fields ]

        /// <summary>
        ///     Data service </summary>
        private EngineDataService _dataService;
        /// <summary>
        ///     Locker </summary>
        private object _lock = new object();

        #endregion [_fields ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public EngineManager(EngineDataService dataService) {
            _dataService = dataService;
        }

        #endregion [ .ctor ]

        #region [ Events ]

        /// <summary>
        ///     Occurred connect/disconnect </summary>
        public event EngineStateChangedHandler EngineStateChanged;
        /// <summary>
        ///     Changed count of queue </summary>
        public event EngineQueueCountChangedHandler EngineQueueCountChanged;
        /// <summary>
        ///     Occurs if manager try to request </summary>
        public event EngineTryRequestHandler EngineTryRequest;
        public event EngineTryRequestHandler EngineRequestEnd;
        /// <summary>
        ///     Occurs if received data via long pooling </summary>
        public event EventResponseReceivedHandler EventReceived;

        #endregion [ Events ]

        #region [ Request ]

        /// <summary>
        ///     Start/Stop Notification flag </summary>
        private bool _requestRun;

        /// <summary>
        ///     Queue of event requests </summary>
        private Queue<EventRequestWrapper> _requests = new Queue<EventRequestWrapper>();

        /// <summary>
        ///     Trys to send event request </summary>
        public void PushEvent(EventRequestWrapper request) {
            _requests.Enqueue(request);
            EngineQueueCountChanged?.Invoke(_requests.Count);
            StartRequest();
        }

        /// <summary>
        ///     Send all requests </summary>
        private void StartRequest() {
            lock (_lock) {
                if (_requestRun) {
                    return;
                }
                _requestRun = true;
            }

            Task.Factory.StartNew(async () => {
                while (_requests.Count > 0) {
                    try {
                        EventRequestWrapper request = _requests.Peek();
                        EngineTryRequest?.Invoke(request.Request.Name, 0, TimeSpan.Zero);
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        EventResponseWrapper result = await _dataService.RequestAsync(request);
                        stopwatch.Stop();
                        EngineRequestEnd?.Invoke(request.Request.Name, result.Size, stopwatch.Elapsed);
                        StackLogger.Push($"Request:{request.Request.Name}");
                        if (result.Status == RequestStatus.Success) {
                            // wrapper == request
                            EventRequestWrapper wrapper = _requests.Dequeue();//TODO если ошибка, то будет повторять много раз?
                            stopwatch = Stopwatch.StartNew();
                            await wrapper.InvokeCompletedAsync(result.Response);
                            stopwatch.Stop();
                            if (stopwatch.Elapsed > TimeSpan.FromSeconds(1)) {
                                EngineRequestEnd?.Invoke("UI", 0, stopwatch.Elapsed);
                            }
                            EngineQueueCountChanged?.Invoke(_requests.Count);
                            EngineStateChanged?.Invoke(ConnectionStatus.Connected);
                        } else {
                            _webl.e("EngineManager.Request\n" + result.ErrorMessage);
                            EngineStateChanged?.Invoke(ConnectionStatus.Disconected);
                            Thread.Sleep(5000);
                        }
                    } catch (Exception e) {
                        Debug.WriteLine(e.ToString());
                        _webl.e("EngineManager.Request", e);
                        Thread.Sleep(5000);
                    }
                }
                _requestRun = false;
            });
        }

        #endregion [ Request ]

        #region [ Response ]

        /// <summary>
        ///     Current Event ID </summary>
        private int? _currentEventId;

        /// <summary>
        ///     Start/Stop Notification flag </summary>
        private bool _responseRun;

        /// <summary>
        ///     Stop Notification </summary>
        public void ResponseStop() {
            _responseRun = false;
        }

        /// <summary>
        ///     Start Notification </summary>
        public void StartResponse(EventTypeDto[] eventTypes) {
            lock (_lock) {
                if (_responseRun) {
                    return;
                }
                _responseRun = true;
            }

            Task.Factory.StartNew(async () => {
                while (_responseRun) {
                    try {
                        EventResponseWrapper wrapper = await _dataService.ResponseAsync(_currentEventId, eventTypes.Select(t => (byte)t).ToArray());
                        if (wrapper.Status == RequestStatus.Success) {
#if DEBUG
                            await Task.Delay(TimeSpan.FromSeconds(2));
#endif
                            _currentEventId = wrapper.Response.EventId;
                            EventReceived?.Invoke(wrapper.Response);
                            EngineStateChanged?.Invoke(ConnectionStatus.Connected);
                        } else {
                            if (wrapper.SkipError == false) {
                                _webl.e("EngineManager.Response\n" + wrapper.ErrorMessage);
                            }
                            EngineStateChanged?.Invoke(ConnectionStatus.Disconected);
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                        }
                    } catch (Exception e) {
                        _webl.e("EngineManager.Response", e);
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
            });
        }

        #endregion [ Response ]
    }
}
