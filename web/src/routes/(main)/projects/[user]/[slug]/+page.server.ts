import type {ServerLoadEvent} from "@sveltejs/kit";
import type {ProjectDto, TicketDto} from "../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/services";
import type {RequestEvent} from "@sveltejs/kit";
import {fail} from "@sveltejs/kit";
import {parsePriority} from "$lib/priority";
import {toProblemDetails} from "$lib/problemDetails";
import type {SwaggerException} from "../../../../../gen/planeraClient";
import {CreateTaskModel} from "../../../../../gen/planeraClient";

export async function load({ params, cookies }: ServerLoadEvent) {
    let response: ProjectDto;
    let ticketsResponse: TicketDto[];
    try {
        response = await getProjectClient(cookies).get(params.user!, params.slug!);
        ticketsResponse = await getProjectClient(cookies).getTickets(params.user!, params.slug!);
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);

        return {
            errors: problem.errors,
        };
    }

    return {
        project: structuredClone(response),
        tickets: structuredClone(ticketsResponse),
    };
}
export const actions = {
    default: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            const response = await getProjectClient(cookies).createTicket(
                params.user!,
                params.slug!,
                {
                    title: formData.get("title") as string,
                    description: formData.get("description") as string,
                    priority: parsePriority(formData.get("priority") as string),
                    assigneeIds: formData.getAll("assignee").map(x => x as string),
                } as CreateTaskModel);
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(400, {
                errors: problem?.errors,
            });
        }
    },
};