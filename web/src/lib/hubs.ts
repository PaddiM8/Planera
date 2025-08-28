import {type HubConnection, HubConnectionBuilder} from "@microsoft/signalr";

let userHub: HubConnection | null = null;
let projectHub: HubConnection | null = null;

async function buildHub(name: string): Promise<HubConnection> {
    // Make sure the url is absolute
    const apiUrl = import.meta.env.VITE_PUBLIC_API_URL.match(/^https?:\/\//)
        ? import.meta.env.VITE_PUBLIC_API_URL
        : window.location.origin + import.meta.env.VITE_PUBLIC_API_URL
    return new HubConnectionBuilder()
        .withUrl(`${apiUrl}/hubs/${name}`)
        .withAutomaticReconnect()
        .build();
}

export async function startUserHub(): Promise<HubConnection> {
    if (!userHub) {
        userHub = await buildHub("user");
        await userHub.start();
    }

    return userHub;
}

export async function startProjectHub(): Promise<HubConnection> {
    if (!projectHub) {
        projectHub = await buildHub("project");
        await projectHub.start();
    }

    return projectHub;
}
