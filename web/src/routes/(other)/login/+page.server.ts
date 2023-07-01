import type {RequestEvent} from "@sveltejs/kit";
import {fail, redirect} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/services";
import type {LoginModel, AuthenticationResult, SwaggerException} from "../../../gen/planeraClient";
import {toProblemDetails} from "$lib/problemDetails";

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
                username: formData.get("username"),
                errors: problem?.errors,
            });
        }

        const cookieOptions: any = {
            httpOnly: true,
            sameSite: "strict",
            secure: false,
            path: "/",
            maxAge: 60 * 60 * 24 * 90
        };
        cookies.set("token", response.token, cookieOptions);

        const user: User = {
            username: response.username,
            email: response.email,
        };
        cookies.set("user", JSON.stringify(user), cookieOptions);

        throw redirect(302, "/");
    }
}