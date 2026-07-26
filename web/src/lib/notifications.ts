import type {HubConnection} from "@microsoft/signalr";
import {dialog} from "$lib/dialog";

export async function subscribeToPushNotifications(userHub: HubConnection, vapidPublicKey: string) {
    if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
        return;
    }

    // If notificationsAllowed has already been set, it has already been handled.
    if (localStorage.getItem("notificationsAllowed")) {
        return;
    }

    if (!await requestPermission()) {
        localStorage.setItem("notificationsAllowed", "false");

        return;
    }

    localStorage.setItem("notificationsAllowed", "true");

    const registration = await navigator.serviceWorker.ready;
    let subscription = await registration.pushManager.getSubscription();

    if (!subscription) {
        subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: vapidPublicKey,
        });

        await userHub.invoke("subscribeToPushNotifications", subscription);
    }
}

async function requestPermission() {
    if (await dialog.yesNo("Allow notifications?", "You might, for example, be notified about upcoming deadlines.")) {
        return await Notification.requestPermission() === "granted";
    }
    
    return false;
}