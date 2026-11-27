using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using System;

namespace AutoDispatch.Logic.Services {
    /// <summary>
    ///     Event if manager try to request </summary>
    public delegate void EngineTryRequestHandler(string methodName, long size, TimeSpan time);
    /// <summary>
    ///     Event if queue of requests was changed </summary>
    public delegate void EngineQueueCountChangedHandler(int queueCount);
    /// <summary>
    ///     Event if connection was changed </summary>
    public delegate void EngineStateChangedHandler(ConnectionStatus status);
    /// <summary>
    ///     Сервер ответил 401 (токен истёк, пользователь удалён): true — выполнен повторный вход, false — App закрывается </summary>
    public delegate System.Threading.Tasks.Task<bool> SessionExpiredHandler();
    ///// <summary>
    /////     Event in module received delegate </summary>
    ///// <param name="module">Module enum</param>
    //public delegate void ModuleReceivedHandler(ModuleType module);
}
