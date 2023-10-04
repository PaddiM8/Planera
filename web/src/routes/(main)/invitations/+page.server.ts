import {getUserClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {handleProblemForForm} from "$lib/problemDetails";
import type {SwaggerException} from "../../../gen/planeraClient";

export const actions = {
    accept: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).acceptInvitation(
                formData.get("projectId") as string,
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
    decline: async({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getUserClient(cookies).declineInvitation(
                formData.get("projectId") as string,
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    }
};
