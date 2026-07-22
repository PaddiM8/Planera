import type {ServerLoadEvent} from "@sveltejs/kit";
import {type SwaggerException, TicketFilter, type TicketQueryResult, TicketSorting} from "../../gen/planeraClient";
import {getTicketClient} from "$lib/clients";
import {handleProblem} from "$lib/problemDetails";
import {ticketsPerPage} from "./projects/[user]/[slug]/store";
import {sanitizeHtml} from "$lib/formatting";
import {makeImagePathsAbsolute} from "$lib/paths";

export async function load({ cookies, params }: ServerLoadEvent) {
    let response: TicketQueryResult;
    try {
        response = await getTicketClient(cookies).getAll(0, ticketsPerPage, TicketSorting.HighestPriority, TicketFilter.Open);
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    for (const ticket of response.tickets ?? []) {
        ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description ?? ""));
    }

    return {
        sorting: response.sorting,
        filter: response.filter,
        tickets: structuredClone(response.tickets),
    };
}
