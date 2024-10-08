import {redirect, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";

export async function load({ cookies }: ServerLoadEvent) {
    await getAuthenticationClient(cookies).logout();
    cookies.delete("token", { path: "/" });

    throw redirect(302, "/login");
}