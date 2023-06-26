import type {Handle} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";

export const handle: Handle = ({ event, resolve }) => {
    const token = event.cookies.get("token");
    const path = event.url.pathname;
    if (!token && !path.startsWith("/login") && !path.startsWith("/register")) {
        throw redirect(302, "/login");
    }

    return resolve(event);
}