using System.Collections.Concurrent;
using LinqToDB;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Имена пользователей для CreatedByName/LastModifiedByName в DTO (V1 UserService.GetUserName с кэшем).
///     Конвертеры AutoMapper без DI — берут хост через ServiceLocator только на промахе кэша </summary>
public static class UserNameCache {

    private static readonly ConcurrentDictionary<Guid, string> Names = new ConcurrentDictionary<Guid, string>();

    public static string Get(Guid userId) {
        if (userId == Guid.Empty) return null;
        if (Names.TryGetValue(userId, out var name)) {
            return name;
        }
        var host = ServiceLocator.GetHost();
        var user = host.UnitOfWork.Context.GetTable<AdUser>().Where(e => e.Id == userId)
            .Select(e => new { e.UserName, e.Email }).FirstOrDefault();
        // неизвестный id (автор не перенесён) кэшируется как null, чтобы не бить в БД на каждый DTO
        name = user == null ? null : (user.UserName ?? user.Email);
        Names[userId] = name;
        return name;
    }

    public static void Remove(Guid userId) {
        Names.TryRemove(userId, out _);
    }
}
