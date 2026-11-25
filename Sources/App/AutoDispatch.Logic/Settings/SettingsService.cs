using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Settings.Sections;
using AutoCall.Logic.Contracts.Settings.Sections;
using AutoDispatch.Data.Api;
using AutoDispatch.Logic.Contracts.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoCall.Logic.Settings {
    /// <summary>
    ///     Настройки пользователя App — секции UserSetting на сервере (Api/Settings), как RemoteSettingsService AutoCall V3:
    ///     все секции читаются один раз после входа (LoadAsync), Get* — из кэша, Set* меняют секцию в кэше, SaveSettings пишет только
    ///     изменённые (V1 писал целиком gzip-JSON всего UserSettings через WCF svc/SettingsService). Публичные Get*/Set* — как в V1
    ///     ISettingsService, только свойства dispatch (AI/wiki/user-settings.md); свойства AutoCall (DAT, звук, хоткеи, паттерны, ViewType,
    ///     Volume) и дефолты компании (Get/SetDefaultColumnSettings, Get/SetDefaultFilterSettings) не перенесены.
    ///     Перенесённый JSON — Json.NET V1 с $type/$values: читается Newtonsoft без разрешения $type (имена сборок клиента другие).
    ///     Логин и сканер — локально (V1 Properties.Settings; здесь — файл %AppData%\AutoDispatch\settings.json, как LocalSettingsService V3) </summary>
    public class SettingsService {

        #region [ Sections ]

        private const string BoardsSection = "BoardsSettings2";
        private const string DockingLayoutSection = "DockingLayout";
        private const string FiltersColumnsSection = "FiltersColumns";
        private const string DefaultLoadSection = "DefaultLoadSettings";
        private const string DatCompaniesDialogGridViewSection = "DatCompaniesDialogGridView";
        private const string EquipmentsDialogGridViewDriversSection = "EquipmentsDialogGridViewDrivers";
        private const string EquipmentsDialogGridViewTrucksSection = "EquipmentsDialogGridViewTrucks";
        private const string EquipmentsDialogGridViewTrailersSection = "EquipmentsDialogGridViewTrailers";
        private const string EquipmentsDialogGridViewWarehousesSection = "EquipmentsDialogGridViewWarehouses";
        private const string EquipmentsDialogGridViewYardsSection = "EquipmentsDialogGridViewYards";
        private const string SelectedCarrierCompanySection = "SelectedCarrierCompany";
        private const string SelectedBrokerCompanySection = "SelectedBrokerCompany";
        private const string ScanEmailSection = "ScanEmail";
        private const string ScanBWSection = "ScanBW";
        private const string ScanADFSection = "ScanADF";
        private const string SearchTrailerWarehouseModeSection = "SearchTrailerWarehouseMode";
        private const string CallServiceSection = "CallServiceSettings";

        #endregion [ Sections ]

        #region [ Private Fields ]

        /// <summary>
        ///     Без разрешения $type: перенесённые секции записаны Json.NET V1 с именами сборок AutoCall.Logic.Contracts </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private static readonly string LocalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoDispatch", "settings.json");

        private readonly SettingsApi _api;
        private readonly object _lock = new object();
        private Dictionary<string, JToken> _sections;
        private readonly HashSet<string> _dirty = new HashSet<string>();

        #endregion [ Private Fields ]

        #region [ .ctor ]

        public SettingsService(SettingsApi api) {
            _api = api;
        }

        #endregion [ .ctor ]

        #region [ Private Methods ]

        /// <summary>
        ///     Все секции пользователя — один раз после входа (до MainWindow) </summary>
        public async Task LoadAsync() {
            var json = await _api.GetSettingsAsync();
            var sections = String.IsNullOrEmpty(json)
                ? new Dictionary<string, JToken>()
                : JObject.Parse(json).Properties().ToDictionary(p => p.Name, p => p.Value);
            lock (_lock) {
                _sections = sections;
                _dirty.Clear();
            }
        }

        private T Get<T>(string section, T defaultValue = default) {
            lock (_lock) {
                if (_sections == null || _sections.TryGetValue(section, out var token) == false || token.Type == JTokenType.Null) {
                    return defaultValue;
                }
                try {
                    return token.ToObject<T>(JsonSerializer.Create(JsonSettings));
                } catch (Exception e) {
                    _l.w($"Settings section {section} could not be read, defaults are used", e);
                    return defaultValue;
                }
            }
        }

        private void Set<T>(string section, T value) {
            lock (_lock) {
                if (_sections == null) {
                    _sections = new Dictionary<string, JToken>();
                }
                var token = value == null ? JValue.CreateNull() : JToken.FromObject(value, JsonSerializer.Create(JsonSettings));
                if (_sections.TryGetValue(section, out var current) == false || JToken.DeepEquals(current, token) == false) {
                    _sections[section] = token;
                    _dirty.Add(section);
                }
            }
        }

        /// <summary>
        ///     V1 SetSettings — синхронная запись всего UserSettings (WCF); теперь только изменённые секции, в фоне </summary>
        private void SetSettings() {
            _ = SaveChangesAsync();
        }

        private async Task SaveChangesAsync() {
            List<KeyValuePair<string, string>> changes;
            lock (_lock) {
                changes = _dirty.Select(s => new KeyValuePair<string, string>(s, _sections[s].ToString(Formatting.None))).ToList();
                _dirty.Clear();
            }
            foreach (var change in changes) {
                try {
                    await _api.SetSectionAsync(change.Key, change.Value);
                } catch (Exception e) {
                    _webl.e($"Settings section {change.Key} was not saved", e);
                    // секция уйдёт при следующем сохранении
                    lock (_lock) {
                        _dirty.Add(change.Key);
                    }
                }
            }
        }

        private sealed class LocalSettings {
            public string UserName { get; set; }
            public string Scanner { get; set; }
        }

        private static LocalSettings LoadLocal() {
            try {
                if (File.Exists(LocalPath)) {
                    return JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(LocalPath)) ?? new LocalSettings();
                }
            } catch (Exception e) {
                _l.w("Local settings could not be read", e);
            }
            return new LocalSettings();
        }

        private static void SaveLocal(Action<LocalSettings> change) {
            var settings = LoadLocal();
            change(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(LocalPath));
            File.WriteAllText(LocalPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        #endregion [ Private Methods ]

        #region [ Public Methods ]

        #region [ UserName ]

        public void SetUserName(string userName) {
            SaveLocal(s => s.UserName = userName);
        }

        public string GetUserName() {
            return LoadLocal().UserName;
        }

        #endregion [ UserName ]

        #region [ Board ]

        public void SetBoardSettings2(Dictionary<BoardType, BoardSettings2> boardsSettings) {
            Set(BoardsSection, boardsSettings);
        }

        /// <summary>
        ///     Нет секции — дефолты конструктора UserSettings V1 (борды dispatch); дефолты компании V1 не перенесены (AI/wiki/user-settings.md) </summary>
        public Dictionary<BoardType, BoardSettings2> GetBoardsSettings2() {
            return Get<Dictionary<BoardType, BoardSettings2>>(BoardsSection) ?? new Dictionary<BoardType, BoardSettings2>() {
                { BoardType.DispatchCarrier, new BoardSettings2() },
                { BoardType.DispatchBroker, new BoardSettings2() },
                { BoardType.DispatchCarrierAccount, new BoardSettings2() },
                { BoardType.DispatchBrokerAccount, new BoardSettings2() },
            };
        }

        #endregion [ Board ]

        #region [ Filter Columns ]

        public FiltersColumnsSettings GetFiltersColumns(byte table) {
            var filters = Get<Dictionary<byte, FiltersColumnsSettings>>(FiltersColumnsSection);
            return filters != null && filters.ContainsKey(table) ? filters[table] : null;
        }

        public void SetFiltersColumns(byte table, FiltersColumnsSettings filter) {
            var filters = Get<Dictionary<byte, FiltersColumnsSettings>>(FiltersColumnsSection) ?? new Dictionary<byte, FiltersColumnsSettings>();
            filters[table] = filter;
            Set(FiltersColumnsSection, filters);
        }

        #endregion [ Filter Columns ]

        #region [ Docking Layout ]

        public void SetDockingLayout(byte[] binary) {
            Set(DockingLayoutSection, binary);
        }

        public byte[] GetDockingLayout() {
            return Get<byte[]>(DockingLayoutSection);
        }

        #endregion [ Docking Layout ]

        #region [ Windows ]

        public void SetDatCompaniesDialogGridView(byte[] binary) {
            Set(DatCompaniesDialogGridViewSection, binary);
        }

        public byte[] GetDatCompaniesDialogGridView() {
            return Get<byte[]>(DatCompaniesDialogGridViewSection);
        }

        public void SetEquipmentsDialogGridViewDrivers(byte[] binary) {
            Set(EquipmentsDialogGridViewDriversSection, binary);
        }

        public byte[] GetEquipmentsDialogGridViewDrivers() {
            return Get<byte[]>(EquipmentsDialogGridViewDriversSection);
        }

        public void SetEquipmentsDialogGridViewTrucks(byte[] binary) {
            Set(EquipmentsDialogGridViewTrucksSection, binary);
        }

        public byte[] GetEquipmentsDialogGridViewTrucks() {
            return Get<byte[]>(EquipmentsDialogGridViewTrucksSection);
        }

        public void SetEquipmentsDialogGridViewTrailers(byte[] binary) {
            Set(EquipmentsDialogGridViewTrailersSection, binary);
            // Save settings
            SetSettings();
        }

        public byte[] GetEquipmentsDialogGridViewTrailers() {
            return Get<byte[]>(EquipmentsDialogGridViewTrailersSection);
        }

        public void SetEquipmentsDialogGridViewWarehouses(byte[] binary) {
            Set(EquipmentsDialogGridViewWarehousesSection, binary);
            // Save settings
            SetSettings();
        }

        public byte[] GetEquipmentsDialogGridViewWarehouses() {
            return Get<byte[]>(EquipmentsDialogGridViewWarehousesSection);
        }

        public void SetEquipmentsDialogGridViewYards(byte[] binary) {
            Set(EquipmentsDialogGridViewYardsSection, binary);
            // Save settings
            SetSettings();
        }

        public byte[] GetEquipmentsDialogGridViewYards() {
            return Get<byte[]>(EquipmentsDialogGridViewYardsSection);
        }

        #endregion [ Windows]

        #region [ Default Edit Dialog ]

        /// <summary>
        ///     Значения нового груза бордов dispatch (V1 — ветка Dispatch* switch'а; ветки DatPower* — модуль AutoCall) </summary>
        public object GetDefaultPatternSettings(BoardType boardType) {
            switch (boardType) {
                case BoardType.DispatchCarrier:
                case BoardType.DispatchBroker:
                case BoardType.DispatchCarrierAccount:
                case BoardType.DispatchBrokerAccount:
                    return Get<DefaultLoadSettings>(DefaultLoadSection);
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetDefaultPatternSettings(BoardType boardType, object pattern) {
            switch (boardType) {
                case BoardType.DispatchCarrier:
                case BoardType.DispatchBroker:
                case BoardType.DispatchCarrierAccount:
                case BoardType.DispatchBrokerAccount:
                    Set(DefaultLoadSection, (DefaultLoadSettings)pattern);
                    break;
                default:
                    throw new NotImplementedException();
            }
            SetSettings();
        }

        #endregion [ Default Edit Dialog ]

        #region [ Call Service ]

        /// <summary>
        ///     Софтфон (AI/wiki/plan.md «Софтфоны в App»): как V1 — MaxPhoneLines по умолчанию 3, неизвестный тип → AbtoPhone;
        ///     в перечне теперь только типы V3 (None, AutoPhone, AutoPhone v.2) </summary>
        public CallServiceSettings GetCallServiceSettings() {
            byte defaultMaxPhoneLines = 3;
            var settings = Get<CallServiceSettings>(CallServiceSection);
            if (settings != null) {
                // Set default MaxPhoneLines
                if (settings.MaxPhoneLines == 0) {
                    settings.MaxPhoneLines = defaultMaxPhoneLines;
                }
                // sets AbtoPhone if using old service
                var callProviders = new List<CallServiceType>() {
                    CallServiceType.Unknown,
                    CallServiceType.MicroSIP,
                    CallServiceType.AbtoPhone
                };
                if (callProviders.Contains(settings.CallService) == false) {
                    settings.CallService = CallServiceType.AbtoPhone;
                }
                return settings;
            } else {
                return new CallServiceSettings() {
                    CallService = CallServiceType.AbtoPhone,
                    CallType = CallType.SingleCall,
                    MaxPhoneLines = defaultMaxPhoneLines
                };
            }
        }

        public void SetCallServiceSettings(CallServiceSettings callServiceSettings) {
            Set(CallServiceSection, callServiceSettings);
        }

        #endregion [ Call Service ]

        #region [ Selected companies ]

        public Guid? GetSelectedCarrierCompany() {
            return ToGuid(Get<byte[]>(SelectedCarrierCompanySection));
        }

        public Guid? GetSelectedBrokerCompany() {
            return ToGuid(Get<byte[]>(SelectedBrokerCompanySection));
        }

        public void SetSelectedCarrierCompany(Guid? id) {
            Set(SelectedCarrierCompanySection, id?.ToByteArray());
        }

        public void SetSelectedBrokerCompany(Guid? id) {
            Set(SelectedBrokerCompanySection, id?.ToByteArray());
        }

        private static Guid? ToGuid(byte[] binary) {
            if (binary == null || binary.Length == 0) {
                return null;
            }
            try {
                return new Guid(binary);
            } catch (Exception) {
                return null;
            }
        }

        public void SaveSettings() {
            SetSettings();
        }

        /// <summary>
        ///     Запись изменённых секций с ожиданием — при закрытии MainWindow (V1 SaveSettings был синхронным) </summary>
        public Task SaveSettingsAsync() {
            return SaveChangesAsync();
        }

        #endregion [ Selected companies ]

        #region [ Scan ]

        public string GetScanEmail() {
            return Get<string>(ScanEmailSection);
        }

        public void SetScanEmail(string scanEmail) {
            Set(ScanEmailSection, scanEmail);
        }

        public bool GetScanBW() {
            return Get(ScanBWSection, false);
        }

        public void SetScanBW(bool scanBW) {
            Set(ScanBWSection, scanBW);
        }

        /// <summary>
        ///     V1 UserSettings.ScanADF = true по умолчанию </summary>
        public bool GetScanADF() {
            return Get(ScanADFSection, true);
        }

        public void SetScanADF(bool scanADF) {
            Set(ScanADFSection, scanADF);
        }

        public void SetDefaultScanner(string scanner) {
            SaveLocal(s => s.Scanner = scanner);
        }

        public string GetDefaultScanner() {
            return LoadLocal().Scanner;
        }

        #endregion [ Scan ]

        #region [ Search Trailer/Warehouse Mode ]

        public bool GetSearchTrailerWarehouseMode() {
            return Get(SearchTrailerWarehouseModeSection, false);
        }

        public void SetSearchTrailerWarehouseMode(bool value) {
            Set(SearchTrailerWarehouseModeSection, value);
        }

        #endregion [ Search Trailer/Warehouse Mode ]

        #endregion [ Public Methods ]
    }
}
