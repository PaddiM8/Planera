import type {ServerLoadEvent} from "@sveltejs/kit";
import type {TicketDto} from "../../../../../gen/planeraClient";
import {getProjectClient, getTicketClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {parsePriority} from "$lib/priority";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import type {SwaggerException} from "../../../../../gen/planeraClient";
import type {CreateTicketModel} from "../../../../../gen/planeraClient";

export async function load({ cookies, params }: ServerLoadEvent) {
    let response: TicketDto[];
    try {
        response = await getProjectClient(cookies).getTickets(params.user!, params.slug!);
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        tickets: structuredClone(response),
    };
}
export const actions = {
    create: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getTicketClient(cookies).create(
                Number(formData.get("projectId")),
                {
                    title: formData.get("title") as string,
                    description: formData.get("description") as string,
                    priority: parsePriority(formData.get("priority") as string),
                    assigneeIds: formData.getAll("assignee").map(x => x as string),
                } as CreateTicketModel);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};