import {error, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";
import type {SwaggerException} from "../../../gen/planeraClient";
import {toProblemDetails} from "$lib/problemDetails";

export async function load({ cookies, url }: ServerLoadEvent) {
    try {
        await getAuthenticationClient(cookies).sendConfirmationEmail(
            url.searchParams.get("username")?.toString() ?? "",
        );
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);
        throw error(problem.status ?? 400, problem.summary);
    }
}
