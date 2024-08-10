import {EditProjectModel, type SwaggerException} from "../../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {handleProblemForForm} from "$lib/problemDetails";

export const actions = {
    update: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).edit(
                params.user!,
                params.slug!,
                new EditProjectModel({
                    name: formData.get("name")?.toString(),
                    description: formData.get("description")?.toString(),
                    icon: formData.get("icon")?.toString(),
                    enableTicketDescriptions: formData.get("enableTicketDescriptions") == "true",
                    enableTicketAssignees: formData.get("enableTicketAssignees") == "true",
                })
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
    delete: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).remove(formData.get("projectId") as string);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};