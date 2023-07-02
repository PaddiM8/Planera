import type {ServerLoadEvent} from "@sveltejs/kit";
import type {LayoutServerLoad} from "../../.svelte-kit/types/src/routes/$types";
import {getUser} from "$lib/cookieParsing";

export const load = (async ({ cookies }) => {
    return {
        user: getUser(cookies),
    } satisfies App.PageData;
}) satisfies LayoutServerLoad;