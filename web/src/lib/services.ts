import {AuthenticationClient, ProjectClient, IConfig} from "../gen/planeraClient";

const serverUrl = "http://localhost:5065";

function extractToken(cookies: any): IConfig {
    return { token: JSON.parse(cookies.get("token") ?? "{}") };
}

export function getAuthenticationClient(cookies: any): AuthenticationClient {
    return new AuthenticationClient(extractToken(cookies), serverUrl, { fetch });
}

export function getProjectClient(cookies: any): ProjectClient {
    return new ProjectClient(extractToken(cookies), serverUrl, { fetch });
}
