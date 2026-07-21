let timeoutId: NodeJS.Timeout;

export const toast = {
    info: (text: string, timeout: number | null = 2750) => {
        return show("info", text, timeout);
    },
    error: (text: string, timeout: number | null = 2750) => {
        return show("error", text, timeout);
    },
    clear: (element: HTMLElement) => {
        element.classList.remove("shown");
    }
};

function show(type: string, text: string, timeout: number | null): HTMLElement | null {
    const element = document.getElementById("toast");
    if (!element) {
        return null;
    }

    element.querySelector(".text")!.textContent = text;
    // Reset
    element.classList.remove("info");
    element.classList.remove("error");

    element.classList.add(type);
    element.classList.add("shown");

    clearTimeout(timeoutId);
    
    if (timeout) {
        timeoutId = setTimeout(() => {
            element.classList.remove("shown");
        }, timeout);
    }
    
    return element;
}
