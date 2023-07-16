import type {SwaggerException, EditProjectModel} from "../../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {fail, redirect} from "@sveltejs/kit";
import {toProblemDetails} from "$lib/problemDetails";

export const actions = {
    update: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).edit(
                params.user!,
                params.slug!,
                {
                    name: formData.get("name"),
                    description: formData.get("description"),
                    icon: formData.get("icon"),
                } as EditProjectModel
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    },
};