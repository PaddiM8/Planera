import type {Cookies} from "@sveltejs/kit";
import type {UserDto} from "../gen/planeraClient";

export function getUser(cookies: Cookies): UserDto | undefined {
    try {
        const user = cookies.get("user");

        return user ? JSON.parse(user) : undefined;
    } catch {
        return undefined;
    }
}