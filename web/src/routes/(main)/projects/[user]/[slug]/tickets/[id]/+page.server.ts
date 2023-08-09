import type {ServerLoadEvent} from "@sveltejs/kit";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import type {TicketDto, SwaggerException, CreateTicketModel} from "../../../../../../../gen/planeraClient";
import {getTicketClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";

export async function load({ cookies, params }: ServerLoadEvent) {
    let response: TicketDto;
    try {
        response = await getTicketClient(cookies).get(params.user!, params.slug!, Number(params.id!));
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        ticket: structuredClone(response),
    };
}

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getTicketClient(cookies).edit(
                Number(formData.get("projectId")),
                Number(formData.get("ticketId")),
                {
                    title: formData.get("title") as string,
                    description: formData.get("description") as string,
                } as CreateTicketModel);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};