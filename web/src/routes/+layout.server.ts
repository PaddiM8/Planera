import type {LayoutServerLoad} from "../../.svelte-kit/types/src/routes/$types";
import {toProblemDetails} from "$lib/problemDetails";
import type {UserDto, SwaggerException} from "../gen/planeraClient";
import {error} from "@sveltejs/kit";
import {getUserClient} from "$lib/clients";

export const load = (async ({ cookies }) => {
    if (!cookies.get("token")) {
        return {};
    }

    let response: UserDto;
    try {
        response = await getUserClient(cookies).get();
    } catch (ex) {
        const problem = toProblemDetails(ex as SwaggerException);
        throw error(problem.status ?? 400, problem.summary);
    }

    return {
        user: structuredClone(response),
    };
}) satisfies LayoutServerLoad;