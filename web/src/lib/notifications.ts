import type {HubConnection} from "@microsoft/signalr";
import {dialog} from "$lib/dialog";
import {browser} from "$app/environment";

export function checkNotificationsEnabled() {
    if (!browser) {
        return false;
    }

    return Notification.permission === "granted";
}

export async function subscribeToPushNotifications(userHub: HubConnection, vapidPublicKey: string) {
    if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
        return;
    }

    if (Notification.permission === "granted" || Notification.permission === "denied" || window.localStorage.getItem("notificationsDenied") === "true") {
        return;
    }

    if (!await requestPermission()) {
        return;
    }

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
    
    window.localStorage.setItem("notificationsDenied", "true");
    
    return false;
}