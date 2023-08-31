const anonymousPaths = [
    "/login",
    "/register",
    "/forgot-password",
    "/reset-password",
    "/confirm-email",
    "/send-confirmation-email",
    "/last-visited",
];

export function pathRequiresAuthentication(url: URL) {
    return !anonymousPaths.some(x => url.pathname.startsWith(x))
}