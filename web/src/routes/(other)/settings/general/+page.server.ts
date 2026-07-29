import type {RequestEvent, ServerLoadEvent} from "@sveltejs/kit";
import {getUserClient} from "$lib/clients";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import {
    type AccountDto,
    type ChangePasswordModel,
    NotificationKinds,
    type SwaggerException
} from "../../../../gen/planeraClient";

export async function load({ cookies, parent }: ServerLoadEvent) {
    let response: AccountDto;
    try {
        response = await getUserClient(cookies).getAccount();
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }
    
    const { authenticationInfo} = await parent();

    return {
        account: structuredClone(response),
        authenticationInfo: structuredClone(authenticationInfo),
        error: false,
    };
}

export const actions = {
    configureNotifications: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            let notificationKinds = [];
            if (formData.get("notify-deadlines") == "true") {
                notificationKinds.push(NotificationKinds.DeadlineMyTicket);
                notificationKinds.push(NotificationKinds.DeadlineOtherTicket);
            }


            if (formData.get("enable-notifications") == "true") {
                notificationKinds.push(NotificationKinds.Core)
            } else {
                notificationKinds = [];
            }
            await getUserClient(cookies).configureNotifications(notificationKinds);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException, "problem");
        }
    },
};
