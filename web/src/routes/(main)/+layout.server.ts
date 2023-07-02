import type {ServerLoadEvent} from "@sveltejs/kit";
import {getProjectClient} from "$lib/services";
import type {Project} from "../../gen/planeraClient";
import {getUser} from "$lib/cookieParsing";

export async function load({ cookies }: ServerLoadEvent) {
    let response: Project[];
    try {
        response = await getProjectClient(cookies).getAll(getUser(cookies)!.username);
    } catch (ex) {
        return {
            error: true
        };
    }

    return {
        projects: structuredClone(response),
        error: false,
    };
}