import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import {getUserClient} from "$lib/clients";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import type {AccountDto, SwaggerException} from "../../../../gen/planeraClient";

export async function load({ cookies }: ServerLoadEvent) {
    let response: AccountDto;
    try {
        response = await getUserClient(cookies).getAccount();
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    return {
        account: structuredClone(response),
        error: false,
    };
}
