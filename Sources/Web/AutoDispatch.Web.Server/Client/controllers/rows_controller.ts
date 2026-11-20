import { Controller } from "@hotwired/stimulus";

// Список строк формы (Customizes компании dispatch, V1 AddCustomize/RemoveCustomize): add — копия template в list,
// __key__ заменяется уникальным ключом (поля Name[key] + скрытый Name.Index — привязка ASP.NET без сплошных индексов);
// remove — убирает строку [data-rows-item], в которой нажата кнопка.

export default class RowsController extends Controller<HTMLElement> {
    static targets = ["template", "list"];

    declare readonly templateTarget: HTMLTemplateElement;
    declare readonly listTarget: HTMLElement;

    add(): void {
        const html = this.templateTarget.innerHTML.replace(/__key__/g, Date.now().toString());
        this.listTarget.insertAdjacentHTML("beforeend", html);
    }

    remove(event: Event): void {
        (event.currentTarget as HTMLElement).closest("[data-rows-item]")?.remove();
    }
}
