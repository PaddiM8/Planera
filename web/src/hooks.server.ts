import type {Handle} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";

const allowedPaths = [
    "/login",
    "/register",
    "/forgot-password",
    "/reset-password",
]

export const handle: Handle = ({ event, resolve }) => {
    const token = event.cookies.get("token");
    const path = event.url.pathname;
    if (!token && !allowedPaths.some(x => path.startsWith(x))) {
        throw redirect(302, "/login");
    }

    return resolve(event);
}