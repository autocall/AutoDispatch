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
    ///     Message received delegate </summary>
    public delegate void MessageReceivedHandler(MessageDto message);
    ///// <summary>
    /////     Event in module received delegate </summary>
    ///// <param name="module">Module enum</param>
    //public delegate void ModuleReceivedHandler(ModuleType module);
}
