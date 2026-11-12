import { Controller } from "@hotwired/stimulus";

// Форма логина: на время htmx-запроса блокирует кнопку, после ошибки возвращает фокус в пароль.
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

    private onAfter = (): void => {
        this.submitTarget.disabled = false;
        this.submitTarget.classList.remove("loading");
    };
}
