import { Controller } from "@hotwired/stimulus";
import { MapSettings, announceMap, createMap, pinIcon, postToApp, readJson, reloadOnThemeChange, whenGoogleReady } from "../map_common";

// Выбор места для App — Map/SelectLocation (Views/Map/SelectLocation.cshtml), замена Dispatch/SelectLocation V1.
// Клик по карте или выбор подсказки → Map/Location (Safer) → адрес (улица) и город «City, ST ZIP» раздельно, как
// GeocodeHelper.getCityAndAddress V1. Каждый выбор уходит в App сообщением { type: "location", … } через WebView2
// postMessage — вместо разбора HTML страницы по id address/city/latlng в EO.WebBrowser; OK в App берёт последнее.
// Начальная точка из ссылки (место груза/склада) показывается сразу с зумом 18, как в V1. Раскладка и Map options — как у /Map.

declare const google: any;

const SELECTED_COLOR = "#1d4ed8";
const INITIAL_ZOOM = 18;

interface Location {
    latitude: number;
    longitude: number;
    formattedAddress: string;
    address: string;
    city: string;
}

export default class SelectLocationController extends Controller<HTMLElement> {
    static targets = ["map", "panel", "busy", "address", "city", "latlng", "search", "searchClear", "suggestions", "error"];
    static values = { token: String, point: Object, settings: Object, embedded: Boolean };

    declare readonly mapTarget: HTMLElement;
    declare readonly panelTarget: HTMLElement;
    declare readonly busyTarget: HTMLElement;
    declare readonly addressTarget: HTMLElement;
    declare readonly cityTarget: HTMLElement;
    declare readonly latlngTarget: HTMLElement;
    declare readonly searchTarget: HTMLInputElement;
    declare readonly searchClearTarget: HTMLElement;
    declare readonly suggestionsTarget: HTMLElement;
    declare readonly errorTarget: HTMLElement;

    declare readonly tokenValue: string;
    declare readonly pointValue: { latitude: number; longitude: number } | null;
    declare readonly settingsValue: MapSettings;
    declare readonly embeddedValue: boolean;

    private map: any = null;
    private marker: any = null;
    private selected: { lat: number; lng: number } | null = null;
    private searchTimer: number | null = null;
    private suggestions: { placeId: string; display: string }[] = [];
    private activeSuggestion = -1;
    private requestId = 0; // ответ на устаревший клик не перекрывает последний

    connect(): void {
        if (this.embeddedValue == false) reloadOnThemeChange();
        whenGoogleReady(() => this.init());
    }

    private init(): void {
        const settings: MapSettings = { type: 0, traffic: false, weather: false, theme: 0, ...(this.settingsValue as Partial<MapSettings>) };
        const point = this.pointValue && (this.pointValue.latitude || this.pointValue.longitude) ? this.pointValue : null;
        this.map = point
            ? createMap(this.mapTarget, settings, { lat: point.latitude, lng: point.longitude }, INITIAL_ZOOM)
            : createMap(this.mapTarget, settings);
        this.map.addListener("click", (e: any) => this.select(e.latLng.lat(), e.latLng.lng(), false));
        announceMap(this.map);
        if (point) {
            this.select(point.latitude, point.longitude, false);
        }
    }

    // ---------- selection ----------

    private async select(lat: number, lng: number, center: boolean): Promise<void> {
        const id = ++this.requestId;
        this.selected = { lat, lng };
        this.placeMarker(lat, lng);
        if (center) {
            this.map.setCenter({ lat, lng });
            this.map.setZoom(INITIAL_ZOOM);
        }
        this.busyTarget.classList.remove("hidden");
        try {
            const response = await fetch(`/Map/Location?r=${encodeURIComponent(this.tokenValue)}&lat=${lat}&lng=${lng}`);
            if (response.status === 404) {
                this.show({ latitude: lat, longitude: lng, formattedAddress: "", address: "", city: "" });
                return;
            }
            const location: Location = await readJson(response);
            if (id === this.requestId) this.show(location);
        } catch (e) {
            this.showError((e as Error).message);
        } finally {
            if (id === this.requestId) this.busyTarget.classList.add("hidden");
        }
    }

    private show(location: Location): void {
        this.addressTarget.textContent = location.formattedAddress || "Address not found";
        this.cityTarget.textContent = location.address || location.city
            ? `${location.address}${location.address && location.city ? " · " : ""}${location.city}`
            : "";
        this.latlngTarget.textContent = `${location.latitude.toFixed(6)}, ${location.longitude.toFixed(6)}`;
        postToApp({ type: "location", ...location });
    }

    private placeMarker(lat: number, lng: number): void {
        if (this.marker) this.marker.setMap(null);
        this.marker = new google.maps.Marker({ map: this.map, position: { lat, lng }, icon: pinIcon(SELECTED_COLOR), zIndex: 10 });
    }

    // ---------- panel ----------

    togglePanel(): void {
        this.panelTarget.classList.toggle("hidden");
    }

    center(): void {
        if (!this.selected) return;
        this.map.setCenter(this.selected);
        this.map.setZoom(INITIAL_ZOOM);
    }

    // ---------- search (как на /Map) ----------

    search(): void {
        const query = this.searchTarget.value.trim();
        this.searchClearTarget.classList.toggle("hidden", query.length === 0);
        if (this.searchTimer) window.clearTimeout(this.searchTimer);
        if (query.length < 2) {
            this.showSuggestions([]);
            return;
        }
        this.searchTimer = window.setTimeout(async () => {
            try {
                const response = await fetch(`/Map/Places?r=${encodeURIComponent(this.tokenValue)}&search=${encodeURIComponent(query)}`);
                this.showSuggestions(await readJson(response));
            } catch (e) {
                this.showError((e as Error).message);
            }
        }, 400);
    }

    searchKey(event: KeyboardEvent): void {
        const count = this.suggestions.length;
        if (count === 0) return;
        if (event.key === "ArrowDown" || event.key === "ArrowUp") {
            event.preventDefault();
            const step = event.key === "ArrowDown" ? 1 : -1;
            this.highlightSuggestion((this.activeSuggestion + step + count) % count);
        } else if (event.key === "Enter") {
            event.preventDefault();
            this.selectPlace(this.suggestions[Math.max(this.activeSuggestion, 0)]!);
        } else if (event.key === "Escape") {
            this.showSuggestions([]);
        }
    }

    private highlightSuggestion(index: number): void {
        this.activeSuggestion = index;
        this.suggestionsTarget.querySelectorAll("a").forEach((a, i) => a.classList.toggle("bg-base-content/10", i === index));
    }

    searchBlur(): void {
        this.showSuggestions([]);
    }

    searchClear(): void {
        this.searchTarget.value = "";
        this.searchClearTarget.classList.add("hidden");
        this.showSuggestions([]);
    }

    private showSuggestions(items: { placeId: string; display: string }[]): void {
        this.suggestions = items;
        this.activeSuggestion = -1;
        this.suggestionsTarget.innerHTML = "";
        this.suggestionsTarget.classList.toggle("hidden", items.length === 0);
        items.forEach((item, index) => {
            const li = document.createElement("li");
            const a = document.createElement("a");
            a.textContent = item.display;
            a.addEventListener("mouseenter", () => this.highlightSuggestion(index));
            a.addEventListener("mousedown", (e) => { e.preventDefault(); this.selectPlace(item); });
            li.append(a);
            this.suggestionsTarget.append(li);
        });
    }

    // координаты подсказки → тот же путь, что клик по карте: адрес и город с сервера
    private async selectPlace(item: { placeId: string; display: string }): Promise<void> {
        this.searchTarget.value = item.display;
        this.showSuggestions([]);
        try {
            const response = await fetch(`/Map/Places/${encodeURIComponent(item.placeId)}?r=${encodeURIComponent(this.tokenValue)}`);
            const place = await readJson(response);
            await this.select(place.latitude, place.longitude, true);
        } catch (e) {
            this.showError((e as Error).message);
        }
    }

    // ---------- helpers ----------

    private showError(message: string): void {
        this.errorTarget.textContent = message;
        this.errorTarget.classList.remove("hidden");
        window.setTimeout(() => this.errorTarget.classList.add("hidden"), 5000);
    }
}
