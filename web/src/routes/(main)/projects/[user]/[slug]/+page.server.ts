import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import type {CreateTicketModel, SwaggerException, TicketQueryResult} from "../../../../../gen/planeraClient";
import {getTicketClient} from "$lib/clients";
import {parsePriority} from "$lib/priority";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import {ticketsPerPage} from "./store";
import {makeImagePathsAbsolute} from "$lib/paths";
import {sanitizeHtml} from "$lib/formatting";

export async function load({ cookies, params }: ServerLoadEvent) {
    let response: TicketQueryResult;
    try {
        response = await getTicketClient(cookies).getAll(
            params.user!,
            params.slug!,
            0,
            ticketsPerPage,
        );
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    for (const ticket of response.tickets) {
        ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description));
    }

    return {
        sorting: response.sorting,
        filter: response.filter,
        tickets: structuredClone(response.tickets),
    };
}
export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getTicketClient(cookies).create(
                formData.get("projectId") as string,
                {
                    title: (formData.get("title") as string).trim(),
                    description: (formData.get("description") as string).trim(),
                    priority: parsePriority(formData.get("priority") as string),
                    assigneeIds: formData.getAll("assignee").map(x => x as string),
                } as CreateTicketModel);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};