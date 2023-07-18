import {getProjectClient} from "$lib/clients";
import {handleProblem} from "$lib/problemDetails";
import type {SwaggerException} from "../../../../../gen/planeraClient";
import type {ProjectDto} from "../../../../../gen/planeraClient";
import type {ServerLoadEvent} from "@sveltejs/kit";

export async function load({ params, cookies }: ServerLoadEvent) {
    let response: ProjectDto;
    try {
        response = await getProjectClient(cookies).get(params.user!, params.slug!);
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        project: structuredClone(response),
    };
}
