using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Common.Loggers;
using AutoDispatch.Data;
using AutoDispatch.Data.Metrics;
using AutoDispatch.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;
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
        /// <summary>
        ///     Сервер ответил 401: подписчик (MainWindow) показывает окно входа </summary>
        public event SessionExpiredHandler SessionExpired;

        #endregion [ Events ]

        #region [ Errors ]

        private Task<bool> _relogin;
        private bool _sessionClosed;

        private static bool IsUnauthorized(EventResponseWrapper result) {
            return result.Exception is HttpRequestException http && http.StatusCode == HttpStatusCode.Unauthorized;
        }

        /// <summary>
        ///     Повтор не поможет: ответ 4xx (кроме 408 и 429) или тело ответа не разбирается. Сеть, таймаут и 5xx — повторяются </summary>
        private static bool IsPermanent(EventResponseWrapper result) {
            if (result.Exception is HttpRequestException http && http.StatusCode is HttpStatusCode code) {
                return (int)code >= 400 && (int)code < 500 && code != HttpStatusCode.RequestTimeout && code != HttpStatusCode.TooManyRequests;
            }
            return result.Exception is Newtonsoft.Json.JsonException;
        }

        /// <summary>
        ///     Повторный вход после 401, один на оба цикла; token — токен, с которым ушёл запрос: если он уже другой,
        ///     вход выполнен другим циклом. false — входа не было, циклы останавливаются </summary>
        private async Task<bool> ReloginAsync(string token) {
            Task<bool> task;
            lock (_lock) {
                if (_sessionClosed) {
                    return false;
                }
                if (UserSession.Current?.AccessToken != token) {
                    return true;
                }
                task = _relogin ??= SessionExpired?.Invoke() ?? Task.FromResult(false);
            }
            bool result = await task;
            lock (_lock) {
                if (_relogin == task) {
                    _relogin = null;
                }
                if (result == false) {
                    _sessionClosed = true;
                }
            }
            return result;
        }

        #endregion [ Errors ]

        #region [ Connection log ]

        /// <summary>
        ///     Запрос к серверу дольше — warning и метрика AppSlowRequests </summary>
        public static readonly TimeSpan SlowRequest = TimeSpan.FromSeconds(5);
        // Обработчик ответа не замеряется в лог: многие открывают модальное окно (Preview Images, выбор склада), и в замер
        // входило время, пока окно открыто (app-testing 9.7: «FilesLoad 43 s»). Остался статус V1 «UI Ns» (EngineRequestEnd)

        private static Serilog.ILogger Logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Dispatch);

        private bool _disconnected;
        private DateTime _disconnectedAt;

        /// <summary>
        ///     Состояние канала для подписчиков; переход Connected ↔ Disconnected — в лог (причина, сколько не было связи).
        ///     Повторные Disconnected (повтор раз в 5 с) не пишутся — только первый и восстановление </summary>
        private void SetState(ConnectionStatus status, string reason = null) {
            lock (_lock) {
                if (status == ConnectionStatus.Disconected && _disconnected == false) {
                    _disconnected = true;
                    _disconnectedAt = DateTime.UtcNow;
                    Logger.Warning("Connection lost: {Reason}", reason);
                } else if (status == ConnectionStatus.Connected && _disconnected) {
                    _disconnected = false;
                    Logger.Information("Connection restored after {Seconds:F0} s", (DateTime.UtcNow - _disconnectedAt).TotalSeconds);
                }
            }
            EngineStateChanged?.Invoke(status);
        }

        #endregion [ Connection log ]

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
                        string token = UserSession.Current?.AccessToken;
                        EngineTryRequest?.Invoke(request.Request.Name, 0, TimeSpan.Zero);
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        EventResponseWrapper result = await _dataService.RequestAsync(request);
                        stopwatch.Stop();
                        EngineRequestEnd?.Invoke(request.Request.Name, result.Size, stopwatch.Elapsed);
                        StackLogger.Push($"Request:{request.Request.Name}");
                        if (stopwatch.Elapsed > SlowRequest) {
                            ClientMetrics.SlowRequest();
                            Logger.Warning("Slow request {Name}: {Seconds:F1} s, {Size} bytes", request.Request.Name, stopwatch.Elapsed.TotalSeconds, result.Size);
                        }
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
                            SetState(ConnectionStatus.Connected);
                        } else if (IsUnauthorized(result)) {
                            // токен истёк или пользователь удалён: запрос остаётся в очереди и уйдёт с новым токеном
                            SetState(ConnectionStatus.Disconected, $"Request {request.Request.Name}: 401 Unauthorized");
                            if (await ReloginAsync(token) == false) {
                                break;
                            }
                        } else if (IsPermanent(result)) {
                            // запрос завершается ошибкой, как при Status Failed от сервера, — очередь идёт дальше
                            _webl.e($"EngineManager.Request {request.Request.Name}\n" + result.ErrorMessage);
                            EventRequestWrapper wrapper = _requests.Dequeue();
                            await wrapper.InvokeCompletedAsync(new EventResponse() { Status = ResponseStatus.Failed, ErrorMessage = result.ErrorMessage });
                            EngineQueueCountChanged?.Invoke(_requests.Count);
                        } else {
                            // сеть, таймаут, 5xx — повтор раз в 5 с; в лог — один раз на период без связи (SetState), а не каждый повтор
                            SetState(ConnectionStatus.Disconected, $"Request {request.Request.Name}: {result.ErrorMessage}");
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
                        string token = UserSession.Current?.AccessToken;
                        EventResponseWrapper wrapper = await _dataService.ResponseAsync(_currentEventId, eventTypes.Select(t => (byte)t).ToArray());
                        if (wrapper.Status == RequestStatus.Success) {
#if DEBUG
                            await Task.Delay(TimeSpan.FromSeconds(2));
#endif
                            _currentEventId = wrapper.Response.EventId;
                            EventReceived?.Invoke(wrapper.Response);
                            SetState(ConnectionStatus.Connected);
                            if (wrapper.Response.Status == ResponseStatus.Failed) {
                                // сервер ответил ошибкой (сбой опроса, отмена опросом того же пользователя с другого ПК) — без паузы опрос шёл бы непрерывно
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                            }
                        } else if (IsUnauthorized(wrapper)) {
                            SetState(ConnectionStatus.Disconected, "Response: 401 Unauthorized");
                            if (await ReloginAsync(token) == false) {
                                _responseRun = false;
                            }
                        } else {
                            // в лог — один раз на период без связи (SetState), а не каждый повтор раз в 5 с
                            SetState(ConnectionStatus.Disconected, "Response: " + wrapper.ErrorMessage);
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
