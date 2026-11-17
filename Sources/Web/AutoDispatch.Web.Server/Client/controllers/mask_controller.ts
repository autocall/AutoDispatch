import { Controller } from "@hotwired/stimulus";

// Маска ввода (jquery.maskedinput V1): pattern — "(999) 999-9999", "999-99-9999", "99-9999999"; 9 — цифра,
// остальные символы подставляются сами. Вводимые нецифры отбрасываются, лишние цифры обрезаются.

export default class MaskController extends Controller<HTMLInputElement> {
    static values = { pattern: String };

    declare readonly patternValue: string;

    connect(): void {
        this.format();
    }

    format(): void {
        const digits = this.element.value.replace(/\D/g, "");
        if (digits.length === 0) {
            this.element.value = "";
            return;
        }
        let result = "";
        let index = 0;
        for (const char of this.patternValue) {
            if (index >= digits.length) break;
            if (char === "9") {
                result += digits[index++];
            } else {
                result += char;
            }
        }
        this.element.value = result;
    }
}
