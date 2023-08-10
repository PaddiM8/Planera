export const dialog = {
    yesNo: (title: string, message: string): Promise<boolean> => {
        return new Promise((resolve, reject) => {
            const element = document.getElementById("yes-no-dialog");
            if (!element) {
                return reject();
            }

            const backgroundElement = document.getElementById("dialog-background");
            backgroundElement?.classList.add("shown");

            element.querySelector("h1")!.textContent = title;
            element.querySelector(".message")!.textContent = message;
            element.classList.add("shown");
            element.focus();

            const buttons = element.querySelector(".buttons")!.children;
            buttons[0].classList.add("close");
            buttons[0].addEventListener("click", () => {
                resolve(false);
                backgroundElement?.classList.remove("shown");
                element.classList.remove("shown");
            });
            buttons[1].addEventListener("click", () => {
                resolve(true);
                backgroundElement?.classList.remove("shown");
                element.classList.remove("shown");
            });
        });
    }
}