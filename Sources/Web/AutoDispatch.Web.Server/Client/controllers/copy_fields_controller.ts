import { Controller } from "@hotwired/stimulus";

// Копирование полей формы setup packet (CarrierRegistration), замена knockout-логики V1 SetupPacketStep02/06Model.ts:
// - copy: select «same as» — значение select'а + suffix = префикс источника (Corporate + Contact → CorporateContact.*),
//   target — префикс приёмника; пустой выбор очищает приёмник;
// - mirror: чекбокс «Pay To = Main address» — пары приёмник=источник из map, при снятии — очистка;
// - fill: select факторинговой компании — поля приёмника из data-атрибутов выбранного option (map приёмник=атрибут).
// Контроллер вешается на форму, поля ищутся по name.

export default class CopyFieldsController extends Controller<HTMLElement> {

    copy(event: Event): void {
        const select = event.currentTarget as HTMLSelectElement;
        const target = select.dataset.copyFieldsTargetParam!;
        const suffix = select.dataset.copyFieldsSuffixParam ?? "";
        const fields = (select.dataset.copyFieldsFieldsParam ?? "").split(",");
        const source = select.value ? select.value + suffix : null;
        for (const field of fields) {
            this.set(`${target}.${field}`, source ? this.get(`${source}.${field}`) : "");
        }
    }

    mirror(event: Event): void {
        const checkbox = event.currentTarget as HTMLInputElement;
        for (const [target, source] of this.pairs(checkbox.dataset.copyFieldsMapParam)) {
            const field = this.field(target);
            if (!field) continue;
            field.value = checkbox.checked ? this.get(source) : "";
            this.lock(field, checkbox.checked);
        }
    }

    fill(event: Event): void {
        const select = event.currentTarget as HTMLSelectElement;
        const option = select.selectedOptions[0];
        for (const [target, attribute] of this.pairs(select.dataset.copyFieldsMapParam)) {
            this.set(target, option?.dataset[attribute] ?? "");
        }
    }

    private pairs(map: string | undefined): [string, string][] {
        return (map ?? "").split(",").filter(p => p.includes("=")).map(p => p.split("=") as [string, string]);
    }

    private field(name: string): HTMLInputElement | HTMLSelectElement | null {
        return this.element.querySelector<HTMLInputElement | HTMLSelectElement>(`[name="${name}"]`);
    }

    private get(name: string): string {
        return this.field(name)?.value ?? "";
    }

    private set(name: string, value: string): void {
        const field = this.field(name);
        if (field) field.value = value;
    }

    // select не бывает readonly — для него disabled нельзя (значение не уйдёт в POST), поэтому только input
    private lock(field: HTMLInputElement | HTMLSelectElement, locked: boolean): void {
        if (field instanceof HTMLInputElement) field.readOnly = locked;
    }
}
