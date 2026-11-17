import { Controller } from "@hotwired/stimulus";

// Шаг W-9 setup packet (V1 SetupPacketStep07Model.inputW9CompanyType_SelectChanged): тип компании 6 (LLC) показывает
// классификацию, 7 (Other) — свою; 2 (C Corporation) закрывает SSN — у корпорации только EIN

export default class W9Controller extends Controller<HTMLElement> {
    static targets = ["companyType", "taxClassification", "otherClassification", "ssn"];

    declare readonly companyTypeTarget: HTMLSelectElement;
    declare readonly taxClassificationTarget: HTMLElement;
    declare readonly otherClassificationTarget: HTMLElement;
    declare readonly ssnTarget: HTMLInputElement;

    connect(): void {
        this.update();
    }

    update(): void {
        const type = parseInt(this.companyTypeTarget.value, 10);
        this.taxClassificationTarget.classList.toggle("hidden", type !== 6);
        this.otherClassificationTarget.classList.toggle("hidden", type !== 7);
        this.ssnTarget.readOnly = type === 2;
    }
}
