import type {LayoutServerLoad} from "../../.svelte-kit/types/src/routes/$types";
import {handleProblem} from "$lib/problemDetails";
import type {SwaggerException} from "../gen/planeraClient";
import {UserDto} from "../gen/planeraClient";
import {getUserClient, getAuthenticationClient} from "$lib/clients";
import {pathRequiresAuthentication} from "$lib/paths";

export const load = (async ({ cookies, url }) => {
    if (!cookies.get("token") || !pathRequiresAuthentication(url)) {
        const authenticationInfo = await getAuthenticationClient(cookies).getInfo();
        return {
            authenticationInfo: structuredClone(authenticationInfo),
        };
    }

    let response: UserDto;
    try {
        response = await getUserClient(cookies).get();
    } catch (ex: any) {
        const isInvalidToken = ex["headers"] && ex["headers"]["www-authenticate"]?.includes("invalid_token") ||
            ex["response"] && "userId" in JSON.parse(ex["response"])["errors"];
        if (isInvalidToken) {
            console.log("Removing invalid token", ex);
            cookies.delete("token", { path: "/" });
        }

        return handleProblem(ex as SwaggerException);
    }

    return {
        user: structuredClone(response),
    };
}) satisfies LayoutServerLoad;