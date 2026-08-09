import type {ServerLoadEvent} from "@sveltejs/kit";
import {getUserClient} from "$lib/clients";
import type {ProjectDto} from "../../gen/planeraClient";
import {handleProblem} from "$lib/problemDetails";
import type {SwaggerException} from "../../gen/planeraClient";

export async function load({ cookies, parent }: ServerLoadEvent) {
    let response: ProjectDto[];
    let invitationsResponse: ProjectDto[];
    try {
        response = await getUserClient(cookies).getPinnedProjects();
        invitationsResponse = await getUserClient(cookies).getInvitations();
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }
    
    const { authenticationInfo } = await parent();

    return {
        projects: structuredClone(response),
        invitations: structuredClone(invitationsResponse),
        error: false,
        authenticationInfo: authenticationInfo,
    };
}