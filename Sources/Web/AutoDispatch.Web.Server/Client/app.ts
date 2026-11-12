import htmx from "htmx.org";
import "cally";
import { Application } from "@hotwired/stimulus";
import CalendarController from "./controllers/calendar_controller";
import LocaltimeController from "./controllers/localtime_controller";
import LoginController from "./controllers/login_controller";
import MapController from "./controllers/map_controller";
import ModalController from "./controllers/modal_controller";
import ThemeController from "./controllers/theme_controller";

declare global {
    interface Window {
        htmx: typeof htmx;
        Stimulus: Application;
    }
}

// htmx обрабатывает hx-* атрибуты сам после импорта; в window — для отладки и hx-on
window.htmx = htmx;

const application = Application.start();
application.register("calendar", CalendarController);
application.register("localtime", LocaltimeController);
application.register("login", LoginController);
application.register("map", MapController);
application.register("modal", ModalController);
application.register("theme", ThemeController);
window.Stimulus = application;
