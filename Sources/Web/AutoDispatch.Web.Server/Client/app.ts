import htmx from "htmx.org";
import "cally";
import { Application } from "@hotwired/stimulus";
import BoardController from "./controllers/board_controller";
import CalendarController from "./controllers/calendar_controller";
import CopyFieldsController from "./controllers/copy_fields_controller";
import LocaltimeController from "./controllers/localtime_controller";
import LoginController from "./controllers/login_controller";
import MaskController from "./controllers/mask_controller";
import MapController from "./controllers/map_controller";
import MapOptionsController from "./controllers/map_options_controller";
import ModalController from "./controllers/modal_controller";
import OrderStopsController from "./controllers/order_stops_controller";
import RowsController from "./controllers/rows_controller";
import SelectLocationController from "./controllers/select_location_controller";
import ThemeController from "./controllers/theme_controller";
import ToggleController from "./controllers/toggle_controller";
import W9Controller from "./controllers/w9_controller";

declare global {
    interface Window {
        htmx: typeof htmx;
        Stimulus: Application;
    }
}

// htmx обрабатывает hx-* атрибуты сам после импорта; в window — для отладки и hx-on
window.htmx = htmx;

const application = Application.start();
application.register("board", BoardController);
application.register("calendar", CalendarController);
application.register("copy-fields", CopyFieldsController);
application.register("localtime", LocaltimeController);
application.register("login", LoginController);
application.register("mask", MaskController);
application.register("map", MapController);
application.register("map-options", MapOptionsController);
application.register("modal", ModalController);
application.register("order-stops", OrderStopsController);
application.register("rows", RowsController);
application.register("select-location", SelectLocationController);
application.register("theme", ThemeController);
application.register("toggle", ToggleController);
application.register("w9", W9Controller);
window.Stimulus = application;
