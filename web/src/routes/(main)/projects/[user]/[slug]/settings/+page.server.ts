import {EditProjectModel, NotificationKinds, type SwaggerException} from "../../../../../../gen/planeraClient";
import {getProjectClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {handleProblemForForm} from "$lib/problemDetails";

export const actions = {
    update: async ({ request, cookies, params }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).edit(
                params.user!,
                params.slug!,
                new EditProjectModel({
                    name: formData.get("name")?.toString() ?? "-",
                    description: formData.get("description")?.toString() ?? "",
                    icon: formData.get("icon")?.toString(),
                    enableTicketDescriptions: formData.get("enableTicketDescriptions") == "true",
                    enableTicketAssignees: formData.get("enableTicketAssignees") == "true",
                    enableTicketDeadlines: formData.get("enableTicketDeadlines") == "true",
                })
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
    configureNotifications: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            let notificationKinds = [];
            if (formData.get("notify-deadlines") == "all-tickets") {
                notificationKinds.push(NotificationKinds.DeadlineMyTicket);
                notificationKinds.push(NotificationKinds.DeadlineOtherTicket);
            } else if (formData.get("notify-deadlines") == "my-tickets") {
                notificationKinds.push(NotificationKinds.DeadlineMyTicket);
            }

            if (formData.get("enable-notifications") == "true") {
                notificationKinds.push(NotificationKinds.Core)
            } else {
                notificationKinds = [];
            }
            await getProjectClient(cookies).configureUserNotifications(formData.get("projectId") as string, notificationKinds);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException, "problem");
        }
    },
    delete: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getProjectClient(cookies).remove(formData.get("projectId") as string);
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
};