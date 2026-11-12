using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.Dtos.App;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Настройки пользователя по секциям (UserSetting). В AutoDispatch используется только секция Map — настройки карты /Map;
///     секции App (Api/Settings AutoCall V3) не переносятся </summary>
public class UserSettingService : BaseService {

    public const string MapSection = "Map";

    public UserSettingService(ServicesHost host) : base(host) { }

    public async Task<MapSettingsDto> GetMapAsync(int userId) {
        var json = await UnitOfWork.Context.GetTable<UserSetting>().Where(e => e.UserId == userId && e.Section == MapSection)
            .Select(e => e.Json).FirstOrDefaultAsync();
        return json == null ? new MapSettingsDto() : JsonExtension.Deserialize<MapSettingsDto>(json);
    }

    public async Task SetMapAsync(int userId, MapSettingsDto settings) {
        await UnitOfWork.Context.InsertOrReplaceAsync(new UserSetting {
            UserId = userId,
            Section = MapSection,
            Json = JsonExtension.Serialize(settings),
            ModifiedDateTime = DateTime.UtcNow
        });
    }
}
