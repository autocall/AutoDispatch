import { Controller } from "@hotwired/stimulus";
import { METERS_IN_MILE, MapSettings, announceMap, createMap, markerIcon, readJson, reloadOnThemeChange, whenGoogleReady } from "../map_common";

// Остановки трейлера для App — Map/OrderStops (Views/Map/OrderStops.cshtml), замена Dispatch/OrderStops V1.
// Остановки приходят с страницей в порядке PickupOrder/DeliveryOrder; маркер — кружок цветов груза с его номером;
// маршрут между соседними остановками считает сервер (Map/OrderStops/Route → Safer), длина — на клиенте.
// Порядок меняет App drag&drop-ом в своём списке и вызывает window.reorder([{ loadId, type }…]) (V1 — reorder(stops)
// через ExecuteScript): страница переставляет остановки и пересчитывает маршрут.

declare const google: any;

interface Stop { // MapStopDto
    loadId: string;
    index: string;
    type: number; // StopType: 0 — Pickup, 1 — Delivery
    order: number;
    latitude: number;
    longitude: number;
    address: string;
    city: string;
    arrive: string;
    background: string | null;
    foreground: string | null;
}

const LINE_COLOR = "#15803d";
const LINE_WEIGHT = 6;
const DEFAULT_BACKGROUND = "#1d4ed8";
const DEFAULT_FOREGROUND = "#ffffff";

export default class OrderStopsController extends Controller<HTMLElement> {
    static targets = ["map", "panel", "list", "distance", "busy", "error"];
    static values = { token: String, stops: Array, settings: Object, maxStops: Number, embedded: Boolean };

    declare readonly mapTarget: HTMLElement;
    declare readonly panelTarget: HTMLElement;
    declare readonly listTarget: HTMLElement;
    declare readonly distanceTarget: HTMLElement;
    declare readonly busyTarget: HTMLElement;
    declare readonly errorTarget: HTMLElement;

    declare readonly tokenValue: string;
    declare readonly stopsValue: Stop[];
    declare readonly settingsValue: MapSettings;
    declare readonly maxStopsValue: number;
    declare readonly embeddedValue: boolean;

    private map: any = null;
    private stops: Stop[] = [];
    private markers: any[] = [];
    private polylines: any[] = [];
    private requestId = 0;

    connect(): void {
        this.stops = [...this.stopsValue];
        if (this.embeddedValue == false) reloadOnThemeChange();
        (window as any).reorder = (order: { loadId: string; type: number }[]) => this.reorder(order);
        whenGoogleReady(() => this.init());
    }

    disconnect(): void {
        delete (window as any).reorder;
    }

    private init(): void {
        const settings: MapSettings = { type: 0, traffic: false, weather: false, theme: 0, ...(this.settingsValue as Partial<MapSettings>) };
        this.map = createMap(this.mapTarget, settings);
        announceMap(this.map);
        this.renderMarkers();
        this.renderPanel();
        this.route(true);
    }

    // Новый порядок из App: пары (груз, тип) в нужной последовательности; незнакомые пары пропускаются, как в V1
    reorder(order: { loadId: string; type: number }[]): void {
        const reordered: Stop[] = [];
        for (const item of order ?? []) {
            const stop = this.stops.find(s => s.loadId.toLowerCase() === String(item.loadId).toLowerCase() && s.type === Number(item.type));
            if (stop) reordered.push(stop);
        }
        this.stops = reordered;
        this.renderMarkers();
        this.renderPanel();
        this.route(false);
    }

    // ---------- route ----------

    private async route(fit: boolean): Promise<void> {
        const id = ++this.requestId;
        this.polylines.forEach(p => p.setMap(null));
        this.polylines = [];
        this.distanceTarget.textContent = "0 mi";
        if (this.stops.length < 2) {
            if (fit) this.fit();
            return;
        }
        if (this.stops.length > this.maxStopsValue) {
            this.showError(`No more than ${this.maxStopsValue} stops`);
            return;
        }
        this.busyTarget.classList.remove("hidden");
        try {
            const response = await fetch("/Map/OrderStops/Route", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ r: this.tokenValue, points: this.stops.map(s => ({ latitude: s.latitude, longitude: s.longitude })) })
            });
            const data = await readJson(response);
            if (id !== this.requestId) return;
            let meters = 0;
            for (const segment of data.segments ?? []) {
                for (const encoded of segment.polylines) {
                    const path = google.maps.geometry.encoding.decodePath(encoded);
                    meters += google.maps.geometry.spherical.computeLength(path);
                    const polyline = new google.maps.Polyline({ path, strokeColor: LINE_COLOR, strokeOpacity: 1, strokeWeight: LINE_WEIGHT, zIndex: 1 });
                    polyline.setMap(this.map);
                    this.polylines.push(polyline);
                }
            }
            this.distanceTarget.textContent = `${Math.round(meters / METERS_IN_MILE)} mi`;
            if (fit) this.fit();
        } catch (e) {
            this.showError((e as Error).message);
        } finally {
            if (id === this.requestId) this.busyTarget.classList.add("hidden");
        }
    }

    fit(): void {
        const bounds = new google.maps.LatLngBounds();
        for (const marker of this.markers) bounds.extend(marker.getPosition());
        for (const polyline of this.polylines) polyline.getPath().forEach((p: any) => bounds.extend(p));
        if (bounds.isEmpty() == false) this.map.fitBounds(bounds, 60);
    }

    // ---------- markers and panel ----------

    private renderMarkers(): void {
        this.markers.forEach(m => m.setMap(null));
        this.markers = this.stops.map((stop, i) => new google.maps.Marker({
            map: this.map,
            position: { lat: stop.latitude, lng: stop.longitude },
            icon: this.icon(stop),
            title: `${i + 1}. ${this.typeName(stop)} ${stop.index}`,
            zIndex: 10 + i
        }));
    }

    private icon(stop: Stop): any {
        return markerIcon(stop.background ?? DEFAULT_BACKGROUND, stop.foreground ?? DEFAULT_FOREGROUND, stop.index, stop.foreground ?? DEFAULT_FOREGROUND);
    }

    private typeName(stop: Stop): string {
        return stop.type === 0 ? "Pickup" : "Delivery";
    }

    private renderPanel(): void {
        this.listTarget.innerHTML = "";
        this.stops.forEach((stop, i) => {
            if (i > 0) {
                const divider = document.createElement("li");
                divider.className = "divider my-0 h-1";
                this.listTarget.append(divider);
            }
            const li = document.createElement("li");
            li.className = "flex items-start gap-2 py-1";
            const dot = document.createElement("span");
            dot.className = "mt-0.5 block h-6 w-6 shrink-0 rounded-full text-center text-xs font-bold leading-6 border-2";
            dot.style.backgroundColor = stop.background ?? DEFAULT_BACKGROUND;
            dot.style.borderColor = stop.foreground ?? DEFAULT_FOREGROUND;
            dot.style.color = stop.foreground ?? DEFAULT_FOREGROUND;
            dot.textContent = String(i + 1);
            const text = document.createElement("div");
            text.className = "grow min-w-0 cursor-pointer hover:underline";
            text.title = "Center on map";
            text.addEventListener("click", () => this.map.panTo({ lat: stop.latitude, lng: stop.longitude }));
            const label = document.createElement("div");
            label.className = "font-medium";
            label.textContent = `${this.typeName(stop)} ${stop.index}`;
            const address = document.createElement("div");
            address.className = "opacity-70 text-xs break-words";
            address.textContent = [stop.address, stop.city].filter(s => s).join(", ");
            const arrive = document.createElement("div");
            arrive.className = "opacity-70 text-xs";
            arrive.textContent = stop.arrive;
            text.append(label, address, arrive);
            li.append(dot, text);
            this.listTarget.append(li);
        });
    }

    togglePanel(): void {
        this.panelTarget.classList.toggle("hidden");
    }

    private showError(message: string): void {
        this.errorTarget.textContent = message;
        this.errorTarget.classList.remove("hidden");
        window.setTimeout(() => this.errorTarget.classList.add("hidden"), 5000);
    }
}
