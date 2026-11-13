using AutoDispatch.Dtos.Responses;
using System.Threading.Tasks;

namespace AutoDispatch.Dtos {
    /// <summary>
    ///     Event received delegate </summary>
    /// <param name="response">Data of event</param>
    public delegate Task EventResponseReceivedHandler(EventResponse response);
    /// <summary>
    ///     Generic Event received delegate </summary>
    /// <param name="response">Data of event</param>
    public delegate void EventResponseReceivedHandler<in T>(T response) where T : EventResponse;
}
