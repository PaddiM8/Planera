import type {ServerLoadEvent} from "@sveltejs/kit";
import {getProjectClient, getUserClient} from "$lib/clients";
import {getUser} from "$lib/cookieParsing";
import type {ProjectDto} from "../../gen/planeraClient";

export async function load({ cookies }: ServerLoadEvent) {
    let response: ProjectDto[];
    let invitationsResponse: ProjectDto[];
    try {
        response = await getProjectClient(cookies).getAll(getUser(cookies)!.userName);
        invitationsResponse = await getUserClient(cookies).getInvitations();
    } catch (ex) {
        return {
            error: true
        };
    }

    return {
        projects: structuredClone(response),
        invitations: structuredClone(invitationsResponse),
        error: false,
    };
}