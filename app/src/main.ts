function navigate(serverUrl: string) {
    if (!serverUrl.endsWith("/")) {
        serverUrl += "/";
    }

    window.location.href = serverUrl + "last-visited";
}

window.addEventListener("DOMContentLoaded", () => {
    const serverUrl = localStorage.getItem("server-url");
    if (serverUrl) {
        navigate(serverUrl);
    } else {
        document.getElementById("setup")!.classList.add("show");
    }

    document.getElementById("submit-button")!.addEventListener("click", () => {
        const urlInput = document.getElementById("server-url") as HTMLInputElement;
        const url = urlInput!.value;
        localStorage.setItem("server-url", url);
        navigate(url);
    });
});
