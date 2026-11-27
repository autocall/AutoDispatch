using System.Linq.Expressions;
using System.Reflection;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Attributes;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Метод IDispatchService для Api/Dispatch/Request: имя (EventRequest.Name), тип данных, минимальная роль и делегат вызова </summary>
public class DispatchRequestHandler {
    public string Name { get; init; }
    public Type RequestType { get; init; }
    public MinAccessAttribute MinAccess { get; init; }
    public Func<DispatchService, BaseEventRequestData, Task<BaseEventResponseData>> Invoke { get; init; }
    /// <summary>
    ///     Запрос меняет данные или что-то отправляет (не чтение) — метрика ActiveUsers </summary>
    public bool IsChange { get; init; }

    /// <summary>
    ///     Как V1 DispatchApiController.Request: каждая роль пользователя должна проходить MinAccess (иерархия RoleEnum по возрастанию
    ///     или ExtraRoles); без атрибута и без ролей — разрешено </summary>
    public bool HasAccess(IEnumerable<RoleEnum> userRoles) {
        if (MinAccess == null) return true;
        return userRoles.All(role => (int)role >= (int)MinAccess.Role || (MinAccess.ExtraRoles != null && MinAccess.ExtraRoles.Contains(role)));
    }
}

/// <summary>
///     Таблица Name → handler по IDispatchService, строится один раз (AI/wiki/protocol.md): вместо GetMethod + Invoke на каждый запрос V1.
///     Делегаты — скомпилированные Expression, исключения метода приходят без TargetInvocationException.
///     EventNotificationAsync (два параметра) — не здесь, это Api/Dispatch/Response </summary>
public static class DispatchRequestHandlers {

    /// <summary>
    ///     Чтение — Load*, Get* и эти; остальное (Save*, Delete*, *Send, File*, Location*Changed, Undo...) — изменение.
    ///     Объявлено до All: статические поля инициализируются по порядку, Build() его читает </summary>
    private static readonly HashSet<string> ReadNames = ["FileLoad", "FilesLoad", "InsuranceLoad", "UndoPreview"];

    public static readonly IReadOnlyDictionary<string, DispatchRequestHandler> All = Build();

    private static Dictionary<string, DispatchRequestHandler> Build() {
        var result = new Dictionary<string, DispatchRequestHandler>();
        foreach (var method in typeof(IDispatchService).GetMethods()) {
            var parameters = method.GetParameters();
            if (parameters.Length != 1 || typeof(BaseEventRequestData).IsAssignableFrom(parameters[0].ParameterType) == false) continue;
            var returnType = method.ReturnType;
            var isTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
            var resultType = isTask ? returnType.GetGenericArguments()[0] : returnType;
            if (typeof(BaseEventResponseData).IsAssignableFrom(resultType) == false) continue;

            var service = Expression.Parameter(typeof(DispatchService), "service");
            var data = Expression.Parameter(typeof(BaseEventRequestData), "data");
            Expression call = Expression.Call(service, method, Expression.Convert(data, parameters[0].ParameterType));
            call = isTask
                ? Expression.Call(typeof(DispatchRequestHandlers), nameof(AwaitAsync), new[] { resultType }, call)
                : Expression.Call(typeof(Task), nameof(Task.FromResult), new[] { typeof(BaseEventResponseData) }, Expression.Convert(call, typeof(BaseEventResponseData)));
            var invoke = Expression.Lambda<Func<DispatchService, BaseEventRequestData, Task<BaseEventResponseData>>>(call, service, data).Compile();

            result[method.Name] = new DispatchRequestHandler {
                Name = method.Name,
                RequestType = parameters[0].ParameterType,
                MinAccess = method.GetCustomAttribute<MinAccessAttribute>(),
                Invoke = invoke,
                IsChange = IsChangeName(method.Name)
            };
        }
        return result;
    }

    public static bool IsChangeName(string name) {
        return name.StartsWith("Load") == false && name.StartsWith("Get") == false && ReadNames.Contains(name) == false;
    }

    private static async Task<BaseEventResponseData> AwaitAsync<T>(Task<T> task) where T : BaseEventResponseData {
        return await task;
    }
}
