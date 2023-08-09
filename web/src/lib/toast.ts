let timeoutId: number;

export const toast = {
    info: (text: string, timeout = 2750) => {
        show("info", text, timeout);
    },
    error: (text: string, timeout = 2750) => {
        show("error", text, timeout);
    },
};

function show(type: string, text: string, timeout: number) {
    const element = document.getElementById("toast");
    if (!element) {
        return;
    }

    element.querySelector(".text")!.textContent = text;
    // Reset
    element.classList.remove("info");
    element.classList.remove("error");

    element.classList.add(type);
    element.classList.add("shown");

    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
        element.classList.remove("shown");
    }, timeout);
}