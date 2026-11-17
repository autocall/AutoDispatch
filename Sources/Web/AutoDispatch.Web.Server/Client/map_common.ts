// Общее для страниц карт App (Map/SelectLocation, Map/OrderStops): создание Google Map по настройкам пользователя
// (тип карты, тема — как в map_controller.ts), маркеры-кружки, чтение JSON-ответов сервера, сообщения в WebView2 App.
// map_controller.ts (/Map) не трогается — у него свои слои и панели.

declare const google: any;

export interface MapSettings { // MapSettingsDto
    type: number; // MapTypeEnum: 1 — Roadmap, 2 — Hybrid
    traffic: boolean;
    weather: boolean;
    theme: number; // MapThemeEnum: 1 — Light, 2 — Dark
}

export const HYBRID = 2;
export const DARK_THEME = "business"; // data-theme страницы
export const ICON_SIZE = 30;
export const METERS_IN_MILE = 1609.344;

const DEFAULT_CENTER = { lat: 41.657989, lng: -97.174127 };
const DEFAULT_ZOOM = 5;

// init после загрузки Google Maps JS (callback=__mapReady в разметке)
export function whenGoogleReady(init: () => void): void {
    if (typeof google !== "undefined" && google.maps?.Map) {
        init();
    } else {
        document.addEventListener("map:ready", init, { once: true });
    }
}

export function createMap(element: HTMLElement, settings: MapSettings, center = DEFAULT_CENTER, zoom = DEFAULT_ZOOM): any {
    const dark = document.documentElement.dataset.theme === DARK_THEME;
    const map = new google.maps.Map(element, {
        center,
        zoom,
        mapTypeId: settings.type === HYBRID ? "hybrid" : "roadmap",
        colorScheme: dark ? google.maps.ColorScheme.DARK : google.maps.ColorScheme.LIGHT,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
        cameraControl: false,
        clickableIcons: false,
        zoomControl: true,
        zoomControlOptions: { position: google.maps.ControlPosition.RIGHT_CENTER }
    });
    if (settings.traffic) {
        new google.maps.TrafficLayer().setMap(map);
    }
    return map;
}

// В браузере смена темы сайта перезагружает страницу: colorScheme задаётся только при создании карты (как /Map)
export function reloadOnThemeChange(): void {
    new MutationObserver(() => window.location.reload())
        .observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
}

// Круг с подписью, как маркеры /Map; цвет заливки и обводки — свои (у остановок — цвета груза)
export function markerIcon(fill: string, stroke: string, label: string, textColor = "#fff"): any {
    const size = label.length > 2 ? 6 : label.length > 1 ? 7 : 9;
    const y = (8.5 + size * 0.36).toFixed(2);
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 17 17">` +
        `<circle cx="8.5" cy="8.5" r="8.5" fill="${stroke}"/><circle cx="8.5" cy="8.5" r="7.5" fill="#fff"/>` +
        `<circle cx="8.5" cy="8.5" r="6.5" fill="${fill}"/>` +
        `<text x="8.5" y="${y}" fill="${textColor}" font-family="Arial, sans-serif" font-size="${size}" font-weight="bold" text-anchor="middle">${label}</text></svg>`;
    return {
        url: "data:image/svg+xml;charset=UTF-8," + encodeURIComponent(svg),
        scaledSize: new google.maps.Size(ICON_SIZE, ICON_SIZE),
        anchor: new google.maps.Point(ICON_SIZE / 2, ICON_SIZE / 2)
    };
}

// Капля для выбранного места
export function pinIcon(color: string): any {
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 17 17">` +
        `<circle cx="8.5" cy="8.5" r="8.5" fill="${color}"/><circle cx="8.5" cy="8.5" r="7.5" fill="#fff"/>` +
        `<circle cx="8.5" cy="8.5" r="6.5" fill="${color}"/>` +
        `<path fill="#fff" d="M8.5 3.5a3.2 3.2 0 0 1 3.2 3.2c0 2.4-3.2 6.3-3.2 6.3S5.3 9.1 5.3 6.7a3.2 3.2 0 0 1 3.2-3.2z"/>` +
        `<circle cx="8.5" cy="6.7" r="1.2" fill="${color}"/></svg>`;
    return {
        url: "data:image/svg+xml;charset=UTF-8," + encodeURIComponent(svg),
        scaledSize: new google.maps.Size(ICON_SIZE, ICON_SIZE),
        anchor: new google.maps.Point(ICON_SIZE / 2, ICON_SIZE / 2)
    };
}

export async function readJson(response: Response): Promise<any> {
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

// Результат страницы для App: WebView2 получает его в WebMessageReceived (замена разбора HTML страницы в V1 EO.WebBrowser);
// в браузере сообщения некому принимать
export function postToApp(message: object): void {
    const webview = (window as any).chrome?.webview;
    if (webview?.postMessage) {
        webview.postMessage(message);
    }
}
