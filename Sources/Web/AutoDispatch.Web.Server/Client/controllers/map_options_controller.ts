import { Controller } from "@hotwired/stimulus";
import { DARK_THEME, HYBRID, MapSettings, readJson } from "../map_common";

// Панель «Map options» страниц карт App (Views/Map/_MapOptionsPart.cshtml): тип карты, Traffic, Weather, тема — то же, что у /Map
// (map_controller.ts), чтобы Map/SelectLocation и Map/OrderStops выглядели и настраивались как Show Trip. Карту страница отдаёт
// событием map:created (announceMap в map_common.ts); настройки — секция Map в UserSetting пользователя ссылки (Map/Settings).

declare const google: any;

const ROADMAP = 1;
const DARK = 2;
const LIGHT = 1;
const WEATHER_MAX_ZOOM = 9; // радар виден только на мелком масштабе, как на /Map

export default class MapOptionsController extends Controller<HTMLElement> {
    static targets = ["options", "typeRoadmap", "typeHybrid", "traffic", "weather", "dark"];
    static values = { token: String, settings: Object, aerisId: String, aerisSecret: String };

    declare readonly optionsTarget: HTMLElement;
    declare readonly typeRoadmapTarget: HTMLElement;
    declare readonly typeHybridTarget: HTMLElement;
    declare readonly trafficTarget: HTMLInputElement;
    declare readonly weatherTarget: HTMLInputElement;
    declare readonly darkTarget: HTMLInputElement;
    declare readonly hasDarkTarget: boolean;

    declare readonly tokenValue: string;
    declare readonly settingsValue: MapSettings;
    declare readonly aerisIdValue: string;
    declare readonly aerisSecretValue: string;

    private map: any = null;
    private settings: MapSettings = { type: 0, traffic: false, weather: false, theme: 0 };
    private trafficLayer: any = null;
    private weatherLayer: any = null;

    connect(): void {
        this.settings = { ...this.settings, ...(this.settingsValue as Partial<MapSettings>) };
        this.applySettingsToControls();
    }

    // map:created@window — карта страницы создана
    attach(event: CustomEvent): void {
        this.map = event.detail.map;
        this.map.addListener("zoom_changed", () => this.applyWeather());
        this.applyTraffic();
        this.applyWeather();
    }

    toggleOptions(): void {
        this.optionsTarget.classList.toggle("hidden");
    }

    setType(event: Event): void {
        this.settings.type = (event.currentTarget as HTMLElement).dataset.type === "hybrid" ? HYBRID : ROADMAP;
        this.map?.setMapTypeId(this.settings.type === HYBRID ? "hybrid" : "roadmap");
        this.applySettingsToControls();
        this.saveSettings();
    }

    toggleTraffic(): void {
        this.settings.traffic = this.trafficTarget.checked;
        this.applyTraffic();
        this.saveSettings();
    }

    toggleWeather(): void {
        this.settings.weather = this.weatherTarget.checked;
        this.applyWeather();
        this.saveSettings();
    }

    // тема применяется перезагрузкой: colorScheme карты задаётся только при создании
    async toggleTheme(): Promise<void> {
        this.settings.theme = this.darkTarget.checked ? DARK : LIGHT;
        await this.saveSettings();
        window.location.reload();
    }

    private applySettingsToControls(): void {
        this.typeRoadmapTarget.classList.toggle("border-primary", this.settings.type !== HYBRID);
        this.typeHybridTarget.classList.toggle("border-primary", this.settings.type === HYBRID);
        this.trafficTarget.checked = this.settings.traffic;
        this.weatherTarget.checked = this.settings.weather;
        if (this.hasDarkTarget) {
            this.darkTarget.checked = document.documentElement.dataset.theme === DARK_THEME;
        }
    }

    private applyTraffic(): void {
        if (!this.map) return;
        if (this.settings.traffic) {
            this.trafficLayer ??= new google.maps.TrafficLayer();
            this.trafficLayer.setMap(this.map);
        } else if (this.trafficLayer) {
            this.trafficLayer.setMap(null);
        }
    }

    private applyWeather(): void {
        if (!this.map) return;
        const show = this.settings.weather && this.map.getZoom() <= WEATHER_MAX_ZOOM && this.aerisIdValue && this.aerisSecretValue;
        const shown = this.weatherLayer != null && this.map.overlayMapTypes.indexOf(this.weatherLayer) >= 0;
        if (show && !shown) {
            this.weatherLayer ??= new google.maps.ImageMapType({
                getTileUrl: (coord: any, zoom: number) =>
                    `https://maps.aerisapi.com/${this.aerisIdValue}_${this.aerisSecretValue}/radar/${zoom}/${coord.x}/${coord.y}/current.png`,
                tileSize: new google.maps.Size(256, 256),
                opacity: 0.35
            });
            this.map.overlayMapTypes.push(this.weatherLayer);
        } else if (!show && shown) {
            this.map.overlayMapTypes.removeAt(this.map.overlayMapTypes.indexOf(this.weatherLayer));
        }
    }

    private async saveSettings(): Promise<void> {
        try {
            const response = await fetch("/Map/Settings", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ r: this.tokenValue, settings: this.settings })
            });
            await readJson(response);
        } catch (e) {
            console.error(e);
        }
    }
}
