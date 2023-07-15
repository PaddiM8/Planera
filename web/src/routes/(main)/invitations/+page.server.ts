import {getUserClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {fail} from "@sveltejs/kit";
import {toProblemDetails} from "$lib/problemDetails";
import type {SwaggerException} from "../../../gen/planeraClient";

export const actions = {
    accept: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).acceptInvitation(
                Number(formData.get("projectId")),
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    },
    decline: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).declineInvitation(
                Number(formData.get("projectId")),
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    }
};
