/// <reference types="@sveltejs/kit" />
import { build, files, version } from "$service-worker";

const CACHE = `cache-${version}`;
const ASSETS = [
    ...build,
    ...files,
];

self.addEventListener("install", (event: any) => {
    async function addFilesToCache() {
        const cache = await caches.open(CACHE);
        await cache.addAll(ASSETS);
    }

    event.waitUntil(addFilesToCache());
});

self.addEventListener("activate", (event: any) => {
    async function deleteOldCaches() {
        for (const key of await caches.keys()) {
            if (key !== CACHE) await caches.delete(key);
        }
    }

    event.waitUntil(deleteOldCaches());
});

self.addEventListener("fetch", (event: any) => {
    if (event.request.method !== "GET") {
        return;
    }

    if (!event.request.url.startsWith("http")) {
        return;
    }

    // Don't cache SignalR stuff
    if (event.request.url.includes("/api/hubs")) {
        return;
    }

    async function respond() {
        const url = new URL(event.request.url);
        const cache = await caches.open(CACHE);

        // `build`/`files` can always be served from the cache
        if (ASSETS.includes(url.pathname)) {
            return cache.match(url.pathname);
        }

        // For everything else, try the network first, but
        // fall back to the cache if we're offline
        try {
            const response = await fetch(event.request);

            if (response.status === 200) {
                cache.put(event.request, response.clone());
            }

            return response;
        } catch {
            return cache.match(event.request);
        }
    }

    event.respondWith(respond());
});

self.addEventListener("push", async (event: PushEvent) => {
    const payload = event.data.json();
    const options: NotificationOptions = {
        body: payload.body,
        icon: "/favicon.png",
        data: payload.data ?? { url: "/" },
    };

    event.waitUntil(
        self.registration.showNotification(payload.title, options)
    );
});

self.addEventListener("notificationclick", async (event: NotificationEvent) => {
    event.notification.close();

    const targetUrl = new URL(
        event.notification.data?.url || "/",
        self.location.origin
    ).href;
    
    // If a tab with the target URL is already open, focus it
    const clientList = await self.clients.matchAll({ type: "window", includeUncontrolled: true });
    for (const client of clientList) {
        if ("url" in client && client.url.includes(targetUrl) && "focus" in client) {
            console.log("focusing client", client)
            return client.focus();
        }
    }
    
    // ...otherwise navigate to it
    if (self.clients.openWindow) {
        return self.clients.openWindow(targetUrl);
    }
});

self.addEventListener("pushsubscriptionchange", async (event: PushSubscriptionChangeEvent) => {
    const oldEndpoint = event.oldSubscription?.endpoint;
    const newSubscription = await self.registration.pushManager.subscribe({ userVisibleOnly: true });
    await fetch("/api/notifications/refresh", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            oldEndpoint: oldEndpoint,
            newEndpoint: newSubscription.endpoint,
            keys: newSubscription.toJSON().keys,
        }),
    })
});