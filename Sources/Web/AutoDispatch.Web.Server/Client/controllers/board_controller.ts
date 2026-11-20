import { Controller } from "@hotwired/stimulus";

// Поля только брокера или только карриера (knockout visible V1: BoardType() == 1 / 0): select — выбор Board,
// item с data-board="Broker|Carrier" виден, когда выбран этот борд. Применяется при подключении — после перерисовки формы тоже.

export default class BoardController extends Controller<HTMLElement> {
    static targets = ["select", "item"];

    declare readonly selectTarget: HTMLSelectElement;
    declare readonly itemTargets: HTMLElement[];

    connect(): void {
        this.update();
    }

    update(): void {
        const board = this.selectTarget.value;
        this.itemTargets.forEach(e => e.classList.toggle("hidden", e.dataset.board !== board));
    }
}
