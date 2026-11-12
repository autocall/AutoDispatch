import { Controller } from "@hotwired/stimulus";

// <dialog data-controller="modal">: открывается, когда htmx заменил содержимое #modal-content;
// кнопка data-action="modal#close" закрывает; при закрытии содержимое очищается.
export default class ModalController extends Controller<HTMLDialogElement> {
    connect(): void {
        this.element.addEventListener("htmx:afterSwap", this.onSwap);
    }

    disconnect(): void {
        this.element.removeEventListener("htmx:afterSwap", this.onSwap);
    }

    close(): void {
        this.element.close();
    }

    clear(): void {
        const content = this.element.querySelector("#modal-content");
        if (content) content.innerHTML = "";
    }

    private onSwap = (): void => {
        if (this.element.open === false) {
            this.element.showModal();
        }
    };
}
