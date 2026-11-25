using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Extensions;
using AutoDispatch.Data.Api;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCall.WPF.Helpers {
    /// <summary>
    ///     Маршрут на карте сервера — замена MapRouteDialog V1 (WinForms + EO.WebBrowser, ссылка Dispatch/RouteLocation?t=… собиралась
    ///     в App). Ссылку выдаёт сервер (Api/Maps/Link), точки в ней зашифрованы — AI/wiki/public-site.md. Порядок точек — как в V1:
    ///     точки машины (V1 initialOrigin/initialDestination, без «m») по краям — kind Pattern, точки груза («m») между ними — kind Record.
    ///     loadId — груз Show Trip: карта показывает историю его положений (V1 dispatchLoadId).
    ///     Пустой список — пустая карта (V1 RouteLocation2, кнопка Map главного окна) </summary>
    public static class MapRouteHelper {

        public static async Task OpenAsync(
                LocationViewModel initialOrigin = null,
                LocationViewModel initialDestination = null,
                LocationViewModel origin = null,
                LocationViewModel destination = null,
                Guid? loadId = null) {
            var points = new List<MapPointDto>();
            Add(points, initialOrigin, MapPointDto.KindPattern);
            Add(points, origin, MapPointDto.KindRecord);
            Add(points, destination, MapPointDto.KindRecord);
            Add(points, initialDestination, MapPointDto.KindPattern);

            string url;
            try {
                url = await App.Services.GetRequiredService<MapApi>().CreateLinkAsync(points, loadId);
            } catch (Exception exception) {
                Serilog.Log.Error(exception, "Map link request failed");
                MessageBoxEx.Error(exception.GetBaseException().Message);
                return;
            }
            MapWindow.Open(url);
        }

        private static void Add(List<MapPointDto> points, LocationViewModel location, int kind) {
            if (location != null && (location.Latitude != 0 || location.Longitude != 0)) {
                points.Add(new MapPointDto(kind, location.Latitude, location.Longitude));
            }
        }
    }
}
