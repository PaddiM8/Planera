import type {RequestEvent} from "@sveltejs/kit";
import {fail, redirect} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/services";
import type {LoginModel} from "../../gen/planeraClient";

export const actions = {
    default: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        const result = await getAuthenticationClient(cookies).login({
            username: formData.get("username") as string,
            password: formData.get("password") as string,
        } as LoginModel);

        if (result.status != 200) {
            return fail(400, {
                username: formData.get("username"),
                incorrect: true
            });
        }

        const token = await result.data.text();
        cookies.set("token", token, {
            httpOnly: true,
            sameSite: "strict",
            secure: false,
            path: "/",
            maxAge: 60 * 60 * 24 * 90
        });

        throw redirect(302, "/");
    }
}