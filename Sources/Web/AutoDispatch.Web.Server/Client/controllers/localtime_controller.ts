import { Controller } from "@hotwired/stimulus";

// Подсказка с локальным временем браузера у времени в UTC: data-localtime-utc-value="2026-10-23T07:22:22Z"
export default class LocaltimeController extends Controller<HTMLElement> {
    static values = { utc: String };

    declare readonly utcValue: string;

    connect(): void {
        const date = new Date(this.utcValue);
        if (!isNaN(date.getTime())) {
            this.element.title = date.toLocaleString();
        }
    }
}
