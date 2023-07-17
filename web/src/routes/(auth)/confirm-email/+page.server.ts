import {error, redirect, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";
import type {SwaggerException} from "../../../gen/planeraClient";
import {toProblemDetails} from "$lib/problemDetails";

export async function load({ cookies, url }: ServerLoadEvent) {
    try {
        await getAuthenticationClient(cookies).confirmEmail(
            url.searchParams.get("user")?.toString() ?? "",
            url.searchParams.get("token")?.toString() ?? "",
        );
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);
        throw error(problem.status ?? 400, problem.summary);
    }

    throw redirect(302, "/login?emailConfirmed=1");
}
