import { Controller } from "@hotwired/stimulus";

// Cally-календарь в popover внутри GET-формы: выбранная дата (yyyy-MM-dd) — в скрытое поле, форма отправляется.
// Под hx-boost отправка меняет только содержимое страницы и адрес.
export default class CalendarController extends Controller<HTMLFormElement> {
    static targets = ["input", "popover"];

    declare readonly inputTarget: HTMLInputElement;
    declare readonly popoverTarget: HTMLElement;

    select(event: Event): void {
        this.inputTarget.value = (event.target as HTMLInputElement).value;
        this.popoverTarget.hidePopover();
        this.element.requestSubmit();
    }

    // кнопка Clear под календарём: дата снимается, форма отправляется без неё
    clear(): void {
        this.inputTarget.value = "";
        this.popoverTarget.hidePopover();
        this.element.requestSubmit();
    }
}
