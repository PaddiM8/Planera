import type {ServerLoadEvent} from "@sveltejs/kit";
import type {LayoutServerLoad} from "../../.svelte-kit/types/src/routes/$types";

export const load = (async ({ cookies }) => {
    const userJson = cookies.get("user");

    return {
        user: userJson
            ? JSON.parse(userJson) as User
            : undefined,
    } satisfies App.PageData;
}) satisfies LayoutServerLoad;