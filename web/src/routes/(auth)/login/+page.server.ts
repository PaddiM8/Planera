import type {RequestEvent} from "@sveltejs/kit";
import {fail, redirect, type ServerLoadEvent} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";
import type {LoginModel, AuthenticationResult, SwaggerException} from "../../../gen/planeraClient";
import {toProblemDetails} from "$lib/problemDetails";

export async function load({ url }: ServerLoadEvent) {
    if (url.searchParams.get("emailConfirmed") == "1") {
        return {
            emailConfirmed: true,
        }
    }
}

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        let response: AuthenticationResult;
        try {
            response = await getAuthenticationClient(cookies).login({
                username: formData.get("username") as string,
                password: formData.get("password") as string,
            } as LoginModel);
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(problem?.status ?? 400, {
                errors: problem?.errors,
            });
        }

        const cookieOptions: any = {
            httpOnly: true,
            sameSite: "strict",
            secure: false,
            path: "/",
            maxAge: 60 * 60 * 24 * 365
        };
        cookies.set("token", response.token, cookieOptions);

        throw redirect(302, "/");
    }
}