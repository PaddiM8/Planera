import type {ServerLoadEvent} from "@sveltejs/kit";
import type {Project} from "../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/services";
import {getUser} from "$lib/cookieParsing";

export async function load({ params, cookies }: ServerLoadEvent) {
    let response: Project;
    try {
        response = await getProjectClient(cookies).get(params.user!, params.slug!);
    } catch (ex) {
        console.log(ex);
        return {
            error: true
        };
    }

    return {
        project: structuredClone(response),
        error: false,
    };
}
