import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import {getUserClient} from "$lib/clients";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import type {AccountDto, SwaggerException} from "../../../../gen/planeraClient";

export async function load({ cookies }: ServerLoadEvent) {
    let response: AccountDto | null;
    try {
        response = await getUserClient(cookies).getPersonalAccessTokenMetadata();
    } catch (ex) {
        response = null;
    }

    return {
        personalAccessTokenMetadata: structuredClone(response),
        error: false,
    };
}

export const actions = {
    createPersonalAccessToken: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            const token = await getUserClient(cookies).createPersonalAccessToken();

            return { token };
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};
