import type {RequestEvent} from "@sveltejs/kit";
import {redirect} from "@sveltejs/kit";
import {getAuthenticationClient} from "$lib/clients";
import type {ResetPasswordModel, SwaggerException} from "../../../gen/planeraClient";
import {handleProblemForForm} from "$lib/problemDetails";

export const actions = {
    default: async ({ request, cookies, url }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getAuthenticationClient(cookies).resetPassword({
                userId: url.searchParams.get("user"),
                resetToken: url.searchParams.get("token"),
                newPassword: formData.get("newPassword"),
                confirmedPassword: formData.get("confirmedPassword"),
            } as ResetPasswordModel);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }

        throw redirect(302, "/login");
    }
}
