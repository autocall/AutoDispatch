import { Controller } from "@hotwired/stimulus";

// Форма логина: на время htmx-запроса блокирует кнопку, после ошибки возвращает фокус в пароль, 429 (лимит nginx) — в alert.
// Разметка: data-controller="login", кнопка data-login-target="submit", пароль data-login-target="password".
export default class LoginController extends Controller<HTMLFormElement> {
    static targets = ["submit", "password"];

    declare readonly submitTarget: HTMLButtonElement;
    declare readonly passwordTarget: HTMLInputElement;
    declare readonly hasPasswordTarget: boolean;

    connect(): void {
        this.element.addEventListener("htmx:beforeRequest", this.onBefore);
        this.element.addEventListener("htmx:afterRequest", this.onAfter);
        if (this.hasPasswordTarget && this.element.dataset.loginError === "true") {
            this.passwordTarget.focus();
            this.passwordTarget.select();
        }
    }

    disconnect(): void {
        this.element.removeEventListener("htmx:beforeRequest", this.onBefore);
        this.element.removeEventListener("htmx:afterRequest", this.onAfter);
    }

    private onBefore = (): void => {
        this.submitTarget.disabled = true;
        this.submitTarget.classList.add("loading");
    };

    private onAfter = (event: Event): void => {
        this.submitTarget.disabled = false;
        this.submitTarget.classList.remove("loading");
        // 429 — лимит nginx (аудит D10, Deploy/nginx): htmx ответ с ошибкой не вставляет, текст { error } — в alert формы
        const xhr = (event as CustomEvent<{ xhr: XMLHttpRequest }>).detail.xhr;
        if (xhr.status === 429) {
            let text = "Too many attempts. Try again in a few minutes.";
            try {
                text = JSON.parse(xhr.responseText).error ?? text;
            } catch {
                // не JSON — текст по умолчанию
            }
            let alert = this.element.querySelector<HTMLElement>("[role=alert]");
            if (alert == null) {
                alert = document.createElement("div");
                alert.setAttribute("role", "alert");
                alert.className = "alert alert-error text-sm";
                this.element.insertBefore(alert, this.element.querySelector("label"));
            }
            alert.textContent = text;
        }
    };
}
