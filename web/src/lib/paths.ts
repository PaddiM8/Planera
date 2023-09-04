const anonymousPaths = [
    "/login",
    "/register",
    "/forgot-password",
    "/reset-password",
    "/confirm-email",
    "/send-confirmation-email",
    "/last-visited",
];
const imageSrcRegex = /(<img[^>]+src=)['"]([^'"]+)['"]/g;
const schemeLength = "planera:".length;
const imageUrlPrefix = ensureTrailingSlash(import.meta.env.VITE_PUBLIC_API_URL) + "files/";

function ensureTrailingSlash(input: string) {
    return input.endsWith("/") ? input : input + "/";
}

export function pathRequiresAuthentication(url: URL) {
    return !anonymousPaths.some(x => url.pathname.startsWith(x))
}

export function makeImagePathsAbsolute(html: string) {
    const replaceScheme = (input: string) => {
        return imageUrlPrefix + input.slice(schemeLength) + "?mimeType=image%2Fpng";
    }

    return html.replaceAll(imageSrcRegex, (match, start, src) => {
        return src.startsWith("planera:")
            ? `${start}'${replaceScheme(src)}'`
            : match;
    });
}

export function makeImagePathsRelative(html: string) {
    const replacePublicUrl = (input: string) =>
        "planera:" + input.slice(imageUrlPrefix.length).split("?")[0];

    return html.replaceAll(imageSrcRegex, (match, start, src) => {
        return src.startsWith(imageUrlPrefix)
            ? `${start}'${replacePublicUrl(src)}'`
            : match;
    });
}