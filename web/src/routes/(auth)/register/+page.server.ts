import {fail, redirect, type RequestEvent} from "@sveltejs/kit";
import type {AuthenticationResult, RegisterModel, SwaggerException} from "../../../gen/planeraClient";
import {getAuthenticationClient} from "$lib/clients";
import {toProblemDetails} from "$lib/problemDetails";

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        let response: AuthenticationResult | null;
        try {
            response = await getAuthenticationClient(cookies).register({
                username: formData.get("username") as string,
                email: formData.get("email") as string,
                password: formData.get("password") as string,
                confirmedPassword: formData.get("confirmedPassword") as string,
            } as RegisterModel);
        } catch (ex) {
            const problem = toProblemDetails(ex as SwaggerException);

            return fail(problem?.status ?? 400, {
                errors: problem?.errors,
            });
        }

        if (!response) {
            return {
                mailSent: true,
            };
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
