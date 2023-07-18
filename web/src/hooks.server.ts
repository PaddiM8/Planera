import type {Handle} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";
import {pathRequiresAuthentication} from "$lib/paths";

export const handle: Handle = ({ event, resolve }) => {
    const token = event.cookies.get("token");
    if (!token && pathRequiresAuthentication(event.url)) {
        throw redirect(302, "/login");
    }

    return resolve(event);
}