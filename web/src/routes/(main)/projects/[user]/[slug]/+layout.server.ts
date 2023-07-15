import {getProjectClient} from "$lib/clients";
import {toProblemDetails} from "$lib/problemDetails";
import type {SwaggerException} from "../../../../../gen/planeraClient";
import type {ProjectDto} from "../../../../../gen/planeraClient";
import type {ServerLoadEvent} from "@sveltejs/kit";
import {error} from "@sveltejs/kit";

export async function load({ params, cookies }: ServerLoadEvent) {
    let response: ProjectDto;
    try {
        response = await getProjectClient(cookies).get(params.user!, params.slug!);
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);
        throw error(problem.status ?? 400, problem.summary);
    }

    return {
        project: structuredClone(response),
    };
}
