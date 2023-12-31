import type {Handle} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";
import {pathRequiresAuthentication} from "$lib/paths";

export const handle: Handle = ({ event, resolve }) => {
    const token = event.cookies.get("token");
    if (!token && pathRequiresAuthentication(event.url)) {
        throw redirect(302, "/login");
    }

    if (token && (event.url.pathname === "/login" || event.url.pathname === "/register")) {
        throw redirect(302, "/");
    }

    return resolve(event);
}