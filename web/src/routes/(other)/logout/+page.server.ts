import {redirect, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/services";

export async function load({ cookies }: ServerLoadEvent) {
    await getAuthenticationClient(cookies).logout();
    cookies.delete("token");
    cookies.delete("user");

    throw redirect(302, "/login");
}