import { Controller } from "@hotwired/stimulus";

const LIGHT = "corporate";
const DARK = "business";
const KEY = "theme";

// Переключатель темы: чекбокс data-theme-target="input" (daisyUI swap). Выбор — в localStorage,
// при загрузке страницы его применяет инлайн-скрипт в _Layout до первой отрисовки.
export default class ThemeController extends Controller {
    static targets = ["input"];

    declare readonly inputTarget: HTMLInputElement;

    connect(): void {
        this.inputTarget.checked = document.documentElement.dataset.theme === DARK;
    }

    toggle(): void {
        const theme = this.inputTarget.checked ? DARK : LIGHT;
        document.documentElement.dataset.theme = theme;
        try {
            localStorage.setItem(KEY, theme);
        } catch {
            // приватный режим — тема живёт до перезагрузки
        }
    }
}
