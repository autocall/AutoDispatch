import { Controller } from "@hotwired/stimulus";

// Чекбокс показывает/прячет блок (knockout visible V1): switch — чекбокс, content — виден при включённом,
// hidden — виден при выключенном, lock — поля только для чтения при включённом (Pay To при факторинге).
// Состояние применяется при подключении — после перерисовки страницы с ошибками валидации тоже верно.

export default class ToggleController extends Controller<HTMLElement> {
    static targets = ["switch", "content", "hidden", "lock"];

    declare readonly switchTarget: HTMLInputElement;
    declare readonly contentTargets: HTMLElement[];
    declare readonly hiddenTargets: HTMLElement[];
    declare readonly lockTargets: HTMLInputElement[];

    connect(): void {
        this.update();
    }

    update(): void {
        const on = this.switchTarget.checked;
        this.contentTargets.forEach(e => e.classList.toggle("hidden", !on));
        this.hiddenTargets.forEach(e => e.classList.toggle("hidden", on));
        this.lockTargets.forEach(e => { e.readOnly = on; });
    }
}
