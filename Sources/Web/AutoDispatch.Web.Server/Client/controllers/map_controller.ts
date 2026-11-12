import { Controller } from "@hotwired/stimulus";

// Карта маршрута /Map (Views/Map/Index.cshtml): Google Maps JS, маршрут и адреса — с сервера (Safer), в каждом запросе
// токен r из ссылки вместо cookie-логина. Поведение как у Dispatch/RouteLocation V1: клик по карте ставит остановку,
// клик по остановке убирает её, маршрут пересчитывается; вид — как Dashboard GpsV4 (слои, поиск, Fit to map).
// Остановки уходят в ссылку (Map/Link, kind Stop) и подставляются в адрес страницы — скопированная ссылка открывает
// карту уже с ними. Настройки (тип карты, слои, тема) — секция Map в UserSetting пользователя ссылки: приходят с страницей,
// сохраняются через Map/Settings. Цвет карты — по data-theme страницы (embedded: из настроек, браузер: тема сайта), тема
// применяется перезагрузкой: colorScheme задаётся только при создании карты. В браузере (embedded=false) свой переключатель
// не показывается, сохранённая тема не меняется, смена темы в навигации тоже перезагружает страницу.

declare const google: any;

type Kind = "pattern" | "record" | "stop";

interface LinkPoint {
    kind: number; // MapPointKindEnum: 1 — Pattern, 2 — Record
    latitude: number;
    longitude: number;
}

interface LatLng {
    lat: number;
    lng: number;
}

interface Settings { // MapSettingsDto
    type: number; // MapTypeEnum: 1 — Roadmap, 2 — Hybrid
    traffic: boolean;
    weather: boolean;
    theme: number; // MapThemeEnum: 1 — Light, 2 — Dark
}

const ROADMAP = 1;
const HYBRID = 2;
const DARK = 2;
const LIGHT = 1;
const DARK_THEME = "business"; // data-theme страницы
const WEATHER_MAX_ZOOM = 9; // радар виден только на мелком масштабе, как в GpsV4
const ICON_SIZE = 30;
const LINE_WEIGHT = 6;
const METERS_IN_MILE = 1609.344;

const COLORS: Record<Kind, string> = { record: "#15803d", pattern: "#dc2626", stop: "#1d4ed8" };

export default class MapController extends Controller<HTMLElement> {
    static targets = ["map", "panel", "points", "distance", "busy", "search", "searchClear", "suggestions",
        "options", "typeRoadmap", "typeHybrid", "traffic", "weather", "dark", "error"];
    static values = { token: String, points: Array, stops: Array, settings: Object, aerisId: String, aerisSecret: String, maxStops: Number,
        embedded: Boolean };

    declare readonly mapTarget: HTMLElement;
    declare readonly panelTarget: HTMLElement;
    declare readonly pointsTarget: HTMLElement;
    declare readonly distanceTarget: HTMLElement;
    declare readonly busyTarget: HTMLElement;
    declare readonly searchTarget: HTMLInputElement;
    declare readonly searchClearTarget: HTMLElement;
    declare readonly suggestionsTarget: HTMLElement;
    declare readonly optionsTarget: HTMLElement;
    declare readonly typeRoadmapTarget: HTMLElement;
    declare readonly typeHybridTarget: HTMLElement;
    declare readonly trafficTarget: HTMLInputElement;
    declare readonly weatherTarget: HTMLInputElement;
    declare readonly darkTarget: HTMLInputElement;
    declare readonly hasDarkTarget: boolean;
    declare readonly errorTarget: HTMLElement;

    declare tokenValue: string; // меняется при остановках: новый токен от Map/Link
    declare readonly pointsValue: LinkPoint[];
    declare readonly stopsValue: { latitude: number; longitude: number }[];
    declare readonly settingsValue: Settings;
    declare readonly aerisIdValue: string;
    declare readonly aerisSecretValue: string;
    declare readonly maxStopsValue: number;
    declare readonly embeddedValue: boolean;

    private map: any = null;
    private settings: Settings = { type: 0, traffic: false, weather: false, theme: 0 };
    private trafficLayer: any = null;
    private weatherLayer: any = null;
    private stops: LatLng[] = [];
    private labels: string[] = []; // подписи точек ссылки, считаются один раз
    private pointMarkers: any[] = [];
    private stopMarkers: any[] = [];
    private polylines: any[] = [];
    private searchMarker: any = null;
    private loading = false;
    private pending = false;
    private linkDirty = false; // остановки изменились — перед маршрутом перевыпустить ссылку
    private addresses = new Map<string, string>();
    private searchTimer: number | null = null;
    private suggestions: { placeId: string; display: string }[] = [];
    private activeSuggestion = -1; // подсвеченная стрелками подсказка

    connect(): void {
        this.settings = { ...this.settings, ...this.settingsValue };
        this.stops = this.stopsValue.map(s => ({ lat: s.latitude, lng: s.longitude }));
        this.labels = this.pointLabels();
        if (this.embeddedValue == false) {
            new MutationObserver(() => window.location.reload())
                .observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
        }
        if (typeof google !== "undefined" && google.maps?.Map) {
            this.init();
        } else {
            document.addEventListener("map:ready", () => this.init(), { once: true });
        }
    }

    // ---------- init ----------

    private init(): void {
        this.map = new google.maps.Map(this.mapTarget, {
            center: { lat: 41.657989, lng: -97.174127 },
            zoom: 5,
            mapTypeId: this.mapTypeId(),
            colorScheme: this.isDark() ? google.maps.ColorScheme.DARK : google.maps.ColorScheme.LIGHT,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: false,
            cameraControl: false,
            clickableIcons: false,
            zoomControl: true,
            zoomControlOptions: { position: google.maps.ControlPosition.RIGHT_CENTER }
        });
        this.map.addListener("click", (e: any) => this.mapClick(e.latLng));
        this.map.addListener("zoom_changed", () => this.applyWeather());

        this.applySettingsToControls();
        if (this.settings.traffic) this.applyTraffic();
        this.applyWeather();

        this.renderPointMarkers();
        this.renderPanel();
        this.load(true);
    }

    // ---------- route ----------

    private mapClick(latLng: any): void {
        if (this.loading) return;
        if (this.stops.length >= this.maxStopsValue) {
            this.showError(`No more than ${this.maxStopsValue} stops`);
            return;
        }
        this.stops.push({ lat: latLng.lat(), lng: latLng.lng() });
        this.linkDirty = true;
        this.load(false);
    }

    private removeStop(index: number): void {
        if (this.loading) return;
        this.stops.splice(index, 1);
        this.linkDirty = true;
        this.load(false);
    }

    // новый токен с остановками в адресе страницы: Copy link / Open in browser в App (через SourceChanged WebView2),
    // адресная строка и перезагрузка (смена темы, F5) открывают карту уже с ними
    private async updateLink(): Promise<void> {
        const response = await fetch("/Map/Link", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ r: this.tokenValue, stops: this.stops.map(s => ({ latitude: s.lat, longitude: s.lng })) })
        });
        const data = await this.readJson(response);
        this.tokenValue = data.token;
        window.history.replaceState(null, "", `?r=${data.token}${this.embeddedValue ? "&embedded=true" : ""}`);
    }

    private async load(fit: boolean): Promise<void> {
        if (this.loading) {
            this.pending = true;
            return;
        }
        this.loading = true;
        this.busyTarget.classList.remove("hidden");
        this.renderStopMarkers();
        this.renderPanel();
        try {
            if (this.linkDirty) {
                await this.updateLink();
                this.linkDirty = false;
            }
            const canRoute = this.pointsValue.length >= 2 || this.pointsValue.length + this.stops.length >= 2;
            if (canRoute) {
                // остановки сервер берёт из ссылки — она уже перевыпущена выше
                const response = await fetch("/Map/Route", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ r: this.tokenValue })
                });
                const data = await this.readJson(response);
                this.renderRoute(data.segments ?? []);
            } else {
                this.renderRoute([]);
            }
            if (fit) this.fit();
        } catch (e) {
            this.showError((e as Error).message);
        } finally {
            this.loading = false;
            this.busyTarget.classList.add("hidden");
            if (this.pending) {
                this.pending = false;
                this.load(false);
            }
        }
    }

    private renderRoute(segments: { kind: number; polylines: string[] }[]): void {
        this.polylines.forEach(p => p.setMap(null));
        this.polylines = [];
        let meters = 0;
        for (const segment of segments) {
            const record = segment.kind === 2;
            for (const encoded of segment.polylines) {
                const path = google.maps.geometry.encoding.decodePath(encoded);
                meters += google.maps.geometry.spherical.computeLength(path);
                // запись — зелёная жирная, паттерн — красный пунктир той же толщины поверх неё
                const polyline = record
                    ? new google.maps.Polyline({ path, strokeColor: COLORS.record, strokeOpacity: 1, strokeWeight: LINE_WEIGHT, zIndex: 1 })
                    : new google.maps.Polyline({
                        path, strokeOpacity: 0, zIndex: 2,
                        icons: [{
                            icon: { path: "M 0,-1 0,1", strokeOpacity: 1, strokeColor: COLORS.pattern, strokeWeight: LINE_WEIGHT, scale: 2 },
                            offset: "0", repeat: "12px"
                        }]
                    });
                polyline.setMap(this.map);
                this.polylines.push(polyline);
            }
        }
        this.distanceTarget.textContent = `${Math.round(meters / METERS_IN_MILE)} mi`;
    }

    fit(): void {
        const bounds = new google.maps.LatLngBounds();
        for (const marker of [...this.pointMarkers, ...this.stopMarkers]) {
            bounds.extend(marker.getPosition());
        }
        for (const polyline of this.polylines) {
            polyline.getPath().forEach((p: any) => bounds.extend(p));
        }
        if (bounds.isEmpty() == false) this.map.fitBounds(bounds, 60);
    }

    // ---------- markers and panel ----------

    private renderPointMarkers(): void {
        this.pointMarkers = this.pointsValue.map((p, i) => new google.maps.Marker({
            map: this.map,
            position: { lat: p.latitude, lng: p.longitude },
            icon: this.icon(p.kind === 2 ? "record" : "pattern", this.labels[i]!),
            zIndex: 10
        }));
    }

    private renderStopMarkers(): void {
        this.stopMarkers.forEach(m => m.setMap(null));
        this.stopMarkers = this.stops.map((s, i) => {
            const marker = new google.maps.Marker({ map: this.map, position: s, icon: this.icon("stop", String(i + 1)), zIndex: 20, title: `Stop ${i + 1}` });
            marker.addListener("click", () => this.removeStop(i));
            return marker;
        });
    }

    // Подписи как в V1: точки паттерна O, D (дальше P1, P2…), точки записи A, B, C…, остановки 1, 2, 3…
    private pointLabels(): string[] {
        const patternLabels = ["O", "D"];
        let patterns = 0;
        let records = 0;
        return this.pointsValue.map(p => p.kind === 2
            ? String.fromCharCode(65 + records++)
            : (patternLabels[patterns++] ?? `P${patterns - 1}`));
    }

    // Круг с белой подписью, как маркеры V1; "location" — капля для найденного адреса.
    // Та же картинка — в списке точек панели Route
    private svg(kind: Kind, label: string | "location"): string {
        const color = COLORS[kind];
        const content = label === "location"
            ? '<path fill="#fff" d="M8.5 3.5a3.2 3.2 0 0 1 3.2 3.2c0 2.4-3.2 6.3-3.2 6.3S5.3 9.1 5.3 6.7a3.2 3.2 0 0 1 3.2-3.2z"/>' +
              `<circle cx="8.5" cy="6.7" r="1.2" fill="${color}"/>`
            : this.text(label);
        return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 17 17">` +
            `<circle cx="8.5" cy="8.5" r="8.5" fill="${color}"/><circle cx="8.5" cy="8.5" r="7.5" fill="#fff"/>` +
            `<circle cx="8.5" cy="8.5" r="6.5" fill="${color}"/>${content}</svg>`;
    }

    // Вертикальное центрирование смещением базовой линии: dominant-baseline в inline-SVG панели не срабатывает,
    // буква уезжала вверх. Высота заглавных Arial ≈ 0.72 em, базовая линия ниже центра на половину этого
    private text(label: string): string {
        const size = label.length > 1 ? 7 : 9;
        const y = (8.5 + size * 0.36).toFixed(2);
        return `<text x="8.5" y="${y}" fill="#fff" font-family="Arial, sans-serif" font-size="${size}" font-weight="bold"` +
            ` text-anchor="middle">${label}</text>`;
    }

    private icon(kind: Kind, label: string | "location"): any {
        return {
            url: "data:image/svg+xml;charset=UTF-8," + encodeURIComponent(this.svg(kind, label)),
            scaledSize: new google.maps.Size(ICON_SIZE, ICON_SIZE),
            anchor: new google.maps.Point(ICON_SIZE / 2, ICON_SIZE / 2)
        };
    }

    // Порядок строк — как на маршруте: остановки вставлены после начала основного отрезка (первый Record,
    // иначе единственный); то же правило, что в MapService.GetRouteAsync
    private renderPanel(): void {
        const points = this.pointsValue;
        const labels = this.labels;
        const rows: { mark: string; label: string; kind: Kind; lat: number; lng: number; stopIndex?: number }[] = [];
        const linkRows = points.map((p, i) => ({
            mark: labels[i] ?? "",
            label: p.kind === 2 ? `Point ${labels[i]}` : labels[i] === "O" ? "Origin" : labels[i] === "D" ? "Destination" : `Point ${labels[i]}`,
            kind: (p.kind === 2 ? "record" : "pattern") as Kind,
            lat: p.latitude,
            lng: p.longitude
        }));
        const stopRows = this.stops.map((s, i) => ({ mark: String(i + 1), label: `Stop ${i + 1}`, kind: "stop" as Kind, lat: s.lat, lng: s.lng, stopIndex: i }));
        let mainIndex = -1;
        for (let i = 0; i + 1 < points.length; i++) {
            if (points[i].kind === 2 && points[i + 1].kind === 2) { mainIndex = i; break; }
        }
        if (mainIndex < 0 && points.length === 2) mainIndex = 0;
        if (points.length === 0) {
            rows.push(...stopRows);
        } else {
            linkRows.forEach((row, i) => {
                rows.push(row);
                if (i === mainIndex) rows.push(...stopRows);
            });
        }

        this.pointsTarget.innerHTML = "";
        for (const row of rows) {
            if (this.pointsTarget.childElementCount > 0) {
                // разделитель как в Map options
                const divider = document.createElement("li");
                divider.className = "divider my-0 h-1";
                this.pointsTarget.append(divider);
            }
            const li = document.createElement("li");
            li.className = "flex items-start gap-2 py-1";
            const dot = document.createElement("span");
            dot.className = "mt-0.5 block h-6 w-6 shrink-0";
            dot.innerHTML = this.svg(row.kind, row.mark);
            // как Locations в V1: клик по точке центрирует карту на ней
            const text = document.createElement("div");
            text.className = "grow min-w-0 cursor-pointer hover:underline";
            text.title = "Center on map";
            text.addEventListener("click", () => this.map.panTo({ lat: row.lat, lng: row.lng }));
            const label = document.createElement("div");
            label.className = "font-medium";
            label.textContent = row.label;
            const address = document.createElement("div");
            address.className = "opacity-70 text-xs break-words";
            address.textContent = `${row.lat.toFixed(5)}, ${row.lng.toFixed(5)}`;
            text.append(label, address);
            li.append(dot, text);
            if (row.stopIndex !== undefined) {
                const remove = document.createElement("button");
                remove.type = "button";
                remove.className = "btn btn-ghost btn-xs btn-circle self-center";
                remove.title = "Remove stop";
                remove.textContent = "✕";
                const index = row.stopIndex;
                remove.addEventListener("click", () => this.removeStop(index));
                li.append(remove);
            }
            this.pointsTarget.append(li);
            this.resolveAddress(row.lat, row.lng, address);
        }
    }

    private async resolveAddress(lat: number, lng: number, element: HTMLElement): Promise<void> {
        const key = `${lat.toFixed(5)},${lng.toFixed(5)}`;
        let address = this.addresses.get(key);
        if (address === undefined) {
            try {
                const response = await fetch(`/Map/Address?r=${encodeURIComponent(this.tokenValue)}&lat=${lat}&lng=${lng}`);
                const data = await this.readJson(response);
                address = typeof data?.address === "string" ? data.address : "";
            } catch {
                address = "";
            }
            this.addresses.set(key, address ?? ""); // ?? — TS не сужает тип после try/catch
        }
        if (address && element.isConnected) {
            element.textContent = address;
        }
    }

    togglePanel(): void {
        this.panelTarget.classList.toggle("hidden");
    }

    // ---------- search ----------

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
                this.showSuggestions(await this.readJson(response));
            } catch (e) {
                this.showError((e as Error).message);
            }
        }, 400);
    }

    // стрелки ходят по подсказкам, Enter выбирает подсвеченную, без подсветки — первую
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

    // подсветка тем же цветом, что hover у menu daisyUI (base-content 10%), мышь и стрелки выглядят одинаково
    private highlightSuggestion(index: number): void {
        this.activeSuggestion = index;
        this.suggestionsTarget.querySelectorAll("a").forEach((a, i) => a.classList.toggle("bg-base-content/10", i === index));
    }

    searchBlur(): void {
        // клик по подсказке фокус не снимает (mousedown с preventDefault), прятать можно сразу
        this.showSuggestions([]);
    }

    searchClear(): void {
        this.searchTarget.value = "";
        this.searchClearTarget.classList.add("hidden");
        this.showSuggestions([]);
        if (this.searchMarker) {
            this.searchMarker.setMap(null);
            this.searchMarker = null;
        }
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

    private async selectPlace(item: { placeId: string; display: string }): Promise<void> {
        this.searchTarget.value = item.display;
        this.showSuggestions([]);
        try {
            const response = await fetch(`/Map/Places/${encodeURIComponent(item.placeId)}?r=${encodeURIComponent(this.tokenValue)}`);
            const place = await this.readJson(response);
            const position = { lat: place.latitude, lng: place.longitude };
            if (this.searchMarker) this.searchMarker.setMap(null);
            this.searchMarker = new google.maps.Marker({ map: this.map, position, icon: this.icon("stop", "location"), title: item.display, zIndex: 30 });
            this.map.setCenter(position);
            this.map.setZoom(12);
        } catch (e) {
            this.showError((e as Error).message);
        }
    }

    // ---------- options ----------

    toggleOptions(): void {
        this.optionsTarget.classList.toggle("hidden");
    }

    setType(event: Event): void {
        this.settings.type = (event.currentTarget as HTMLElement).dataset.type === "hybrid" ? HYBRID : ROADMAP;
        this.map.setMapTypeId(this.mapTypeId());
        this.applySettingsToControls();
        this.saveSettings();
    }

    private mapTypeId(): string {
        return this.settings.type === HYBRID ? "hybrid" : "roadmap";
    }

    private isDark(): boolean {
        return document.documentElement.dataset.theme === DARK_THEME;
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
            this.darkTarget.checked = this.isDark();
        }
    }

    private applyTraffic(): void {
        if (this.settings.traffic) {
            this.trafficLayer ??= new google.maps.TrafficLayer();
            this.trafficLayer.setMap(this.map);
        } else if (this.trafficLayer) {
            this.trafficLayer.setMap(null);
        }
    }

    private applyWeather(): void {
        const show = this.settings.weather && this.map.getZoom() <= WEATHER_MAX_ZOOM && this.aerisIdValue && this.aerisSecretValue;
        const shown = this.map.overlayMapTypes.getLength() > 0;
        if (show && !shown) {
            this.weatherLayer ??= new google.maps.ImageMapType({
                getTileUrl: (coord: any, zoom: number) =>
                    `https://maps.aerisapi.com/${this.aerisIdValue}_${this.aerisSecretValue}/radar/${zoom}/${coord.x}/${coord.y}/current.png`,
                tileSize: new google.maps.Size(256, 256),
                opacity: 0.35
            });
            this.map.overlayMapTypes.push(this.weatherLayer);
        } else if (!show && shown) {
            this.map.overlayMapTypes.clear();
        }
    }

    private async saveSettings(): Promise<void> {
        try {
            const response = await fetch("/Map/Settings", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ r: this.tokenValue, settings: this.settings })
            });
            await this.readJson(response);
        } catch (e) {
            this.showError((e as Error).message);
        }
    }

    // ---------- helpers ----------

    private async readJson(response: Response): Promise<any> {
        const text = await response.text();
        let data: any = null;
        try {
            data = text ? JSON.parse(text) : null;
        } catch {
            data = null;
        }
        if (!response.ok) {
            throw new Error(data?.error ?? (text || `Request failed (${response.status})`));
        }
        return data;
    }

    private showError(message: string): void {
        this.errorTarget.textContent = message;
        this.errorTarget.classList.remove("hidden");
        window.setTimeout(() => this.errorTarget.classList.add("hidden"), 5000);
    }
}
