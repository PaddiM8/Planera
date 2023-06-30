import {redirect, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/services";

export async function load({ cookies, locals }: ServerLoadEvent) {
    await getAuthenticationClient(cookies).logout();
    cookies.delete("token");
    locals.user = undefined;

    throw redirect(302, "/login");
}