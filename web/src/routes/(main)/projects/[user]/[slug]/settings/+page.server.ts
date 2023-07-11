import type {ServerLoadEvent} from "@sveltejs/kit";
import type {ProjectDto, SwaggerException, EditProjectModel} from "../../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/services";
import type {RequestEvent} from "@sveltejs/kit";
import {fail, redirect} from "@sveltejs/kit";
import {toProblemDetails} from "$lib/problemDetails";

export async function load({ params, cookies }: ServerLoadEvent) {
    let response: ProjectDto;
    try {
        response = await getProjectClient(cookies).get(params.user!, params.slug!);
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);

        return {
            errors: problem.errors,
        };
    }

    return {
        project: structuredClone(response),
    };
}
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
                } as EditProjectModel
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }

        throw redirect(302, `/projects/${params.user!}/${params.slug!}`)
    },
    addParticipant: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).addParticipant(
                Number(formData.get("projectId")),
                formData.get("username") as string,
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    },
    removeParticipant: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).removeParticipant(
                Number(formData.get("projectId")),
                formData.get("username") as string,
            );
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    },
};