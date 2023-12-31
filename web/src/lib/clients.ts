import {AuthenticationClient, UserClient, ProjectClient, TicketClient, IConfig, NoteClient} from "../gen/planeraClient";

const serverUrl = import.meta.env.VITE_INTERNAL_API_URL;

function extractToken(cookies: any): IConfig {
    return { token: cookies.get("token") ?? "{}" };
}

function ensureTrailingSlash(input: string) {
    return input.endsWith("/") ? input : input + "/";
}

export function getAvatarUrl(path: string | undefined, size: "small" | "big") {
    if (!path) {
        return undefined;
    }

    return getFileUrl(
        path,
        "image/png",
        size === "big" ? "256" : "32"
    );
}

export function getFileUrl(path: string, mimeType: string, param: string | undefined) {
    const paramString = param ? `&param=${param}` : "";
    return `${ensureTrailingSlash(import.meta.env.VITE_PUBLIC_API_URL)}files/${path}?mimeType=${encodeURIComponent(mimeType)}${paramString}`
}

export function getAuthenticationClient(cookies: any): AuthenticationClient {
    return new AuthenticationClient(extractToken(cookies), serverUrl, { fetch });
}

export function getUserClient(cookies: any): UserClient {
    return new UserClient(extractToken(cookies), serverUrl, { fetch });
}

export function getProjectClient(cookies: any): ProjectClient {
    return new ProjectClient(extractToken(cookies), serverUrl, { fetch });
}

export function getTicketClient(cookies: any): TicketClient {
    return new TicketClient(extractToken(cookies), serverUrl, { fetch });
}

export function getNoteClient(cookies: any): NoteClient {
    return new NoteClient(extractToken(cookies), serverUrl, { fetch });
}
