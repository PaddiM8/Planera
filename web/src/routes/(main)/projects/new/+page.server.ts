import {redirect, type RequestEvent} from "@sveltejs/kit";
import {getProjectClient} from "$lib/clients";
import type {CreateProjectModel, SwaggerException} from "../../../../gen/planeraClient";
import {handleProblemForForm} from "$lib/problemDetails";

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).create({
                name: formData.get("name") as string,
                slug: formData.get("slug") as string,
                description: formData.get("description") as string,
                icon: formData.get("icon") as string,
            } as CreateProjectModel);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }

        throw redirect(
            302,
            `/projects/${formData.get("username")}/${formData.get("slug")}`
        );
    },
};
