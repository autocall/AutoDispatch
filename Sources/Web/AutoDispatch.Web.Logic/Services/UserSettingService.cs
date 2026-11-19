using System.Text.Json;
using LinqToDB;
using LinqToDB.Async;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.Dtos.App;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Настройки пользователя по секциям (UserSetting), как в AutoCall V3: секции App (свойства верхнего уровня UserSettings клиента,
///     содержимое — непрозрачный JSON, AI/wiki/user-settings.md) и Map — настройки карты /Map </summary>
public class UserSettingService : BaseService {

    public const string MapSection = "Map";

    public UserSettingService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Все секции пользователя одним объектом { section: json } или null, если секций нет; битая секция пропускается </summary>
    public async Task<string> GetAllJsonAsync(Guid userId) {
        var sections = await UnitOfWork.Context.GetTable<UserSetting>().Where(e => e.UserId == userId)
            .Select(e => new { e.Section, e.Json }).ToListAsync();
        var valid = sections.Where(s => {
            try {
                JsonDocument.Parse(s.Json).Dispose();
                return true;
            } catch (JsonException) {
                _l.w($"Settings section skipped, invalid JSON: user {userId}, section {s.Section}");
                return false;
            }
        }).ToList();
        if (valid.Count == 0) {
            return null;
        }
        return "{" + String.Join(",", valid.Select(s => $"{JsonConvert.ToString(s.Section)}:{s.Json}")) + "}";
    }

    public async Task SetJsonAsync(Guid userId, string section, string json) {
        await UnitOfWork.Context.InsertOrReplaceAsync(new UserSetting {
            UserId = userId,
            Section = section,
            Json = json,
            ModifiedDateTime = DateTime.UtcNow
        });
    }

    public async Task<MapSettingsDto> GetMapAsync(Guid userId) {
        var json = await UnitOfWork.Context.GetTable<UserSetting>().Where(e => e.UserId == userId && e.Section == MapSection)
            .Select(e => e.Json).FirstOrDefaultAsync();
        return json == null ? new MapSettingsDto() : JsonExtension.Deserialize<MapSettingsDto>(json);
    }

    public Task SetMapAsync(Guid userId, MapSettingsDto settings) {
        return SetJsonAsync(userId, MapSection, JsonExtension.Serialize(settings));
    }
}
