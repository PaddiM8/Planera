import type {Cookies} from "@sveltejs/kit";

export function getUser(cookies: Cookies): User | undefined {
    try {
        const user = cookies.get("user");

        return user ? JSON.parse(user) : undefined;
    } catch {
        return undefined;
    }
}