import type {Handle} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";
import {pathRequiresAuthentication} from "$lib/paths";

export const handle: Handle = ({ event, resolve }) => {
    const acceptLang = event.request.headers.get("Accept-Language");
    event.locals.locale = acceptLang
        ? acceptLang.split(",")[0]
        : "en-GB";
    
    const token = event.cookies.get("token");
    if (!token && pathRequiresAuthentication(event.url)) {
        throw redirect(302, "/login");
    }

    if (token && (event.url.pathname === "/login" || event.url.pathname === "/register")) {
        throw redirect(302, "/");
    }

    return resolve(event);
}