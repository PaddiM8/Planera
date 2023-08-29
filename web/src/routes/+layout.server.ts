import type {LayoutServerLoad} from "../../.svelte-kit/types/src/routes/$types";
import {handleProblem} from "$lib/problemDetails";
import type {UserDto, SwaggerException} from "../gen/planeraClient";
import {getUserClient} from "$lib/clients";
import {pathRequiresAuthentication} from "$lib/paths";

export const load = (async ({ cookies, url }) => {
    if (!cookies.get("token") || !pathRequiresAuthentication(url)) {
        return {};
    }

    let response: UserDto;
    try {
        response = await getUserClient(cookies).get();
    } catch (ex) {
        console.log(ex)
        return handleProblem(ex as SwaggerException);
    }

    return {
        user: structuredClone(response),
    };
}) satisfies LayoutServerLoad;