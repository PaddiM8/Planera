import {getUserClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {fail} from "@sveltejs/kit";
import {handleProblemForForm, toProblemDetails} from "$lib/problemDetails";
import type {SwaggerException} from "../../../gen/planeraClient";

export const actions = {
    accept: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).acceptInvitation(
                Number(formData.get("projectId")),
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
    decline: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).declineInvitation(
                Number(formData.get("projectId")),
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    }
};
