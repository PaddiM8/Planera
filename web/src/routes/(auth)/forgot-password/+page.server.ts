import type {RequestEvent} from "@sveltejs/kit";
import {fail} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";
import type {SwaggerException} from "../../../gen/planeraClient";
import {toProblemDetails} from "$lib/problemDetails";

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getAuthenticationClient(cookies).forgotPassword(
                formData.get("username")?.toString() ?? ""
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(problem?.status ?? 400, {
                errors: problem?.errors,
            });
        }
    }
}
