import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import type {CreateTicketModel, SwaggerException, TicketDto} from "../../../../../gen/planeraClient";
import {TicketSorting} from "../../../../../gen/planeraClient";
import {getTicketClient} from "$lib/clients";
import {parsePriority} from "$lib/priority";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";

export async function load({ cookies, params }: ServerLoadEvent) {
    let response: TicketDto[];
    try {
        response = await getTicketClient(cookies).getAll(params.user!, params.slug!);
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        tickets: structuredClone(response),
    };
}
export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
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