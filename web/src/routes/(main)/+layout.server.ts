import type {ServerLoadEvent} from "@sveltejs/kit";
import {getProjectClient, getUserClient} from "$lib/clients";
import type {ProjectDto} from "../../gen/planeraClient";
import {handleProblem} from "$lib/problemDetails";
import type {SwaggerException} from "../../gen/planeraClient";

export async function load({ cookies }: ServerLoadEvent) {
    let response: ProjectDto[];
    let invitationsResponse: ProjectDto[];
    try {
        response = await getProjectClient(cookies).getAll();
        invitationsResponse = await getUserClient(cookies).getInvitations();
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        projects: structuredClone(response),
        invitations: structuredClone(invitationsResponse),
        error: false,
    };
}