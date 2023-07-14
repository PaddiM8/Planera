import {getUserClient} from "$lib/clients";
import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import type {ProjectDto} from "../../../gen/planeraClient";
import {fail} from "@sveltejs/kit";
import {toProblemDetails} from "$lib/problemDetails";
import type {SwaggerException} from "../../../gen/planeraClient";

export async function load({ cookies }: ServerLoadEvent) {
    let response: ProjectDto[];
    try {
        response = await getUserClient(cookies).getInvitations();
    } catch (ex) {
        return {
            error: true
        };
    }

    return {
        invitations: structuredClone(response),
        error: false,
    };
}
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
