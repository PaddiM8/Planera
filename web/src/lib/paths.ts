const anonymousPaths = [
    "/login",
    "/register",
    "/forgot-password",
    "/reset-password",
    "/confirm-email",
    "/send-confirmation-email",
];

export function pathRequiresAuthentication(url: URL) {
    return !anonymousPaths.some(x => url.pathname.startsWith(x))
}