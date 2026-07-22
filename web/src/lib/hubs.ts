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
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
                // If we've been reconnecting for less than 60 seconds in total,
                // wait 1 second between attempts, otherwise 5.
                return retryContext.elapsedMilliseconds < 60000
                    ? 1000
                    : 5000;
            }
        })
        .build();
}

export async function startUserHub(): Promise<HubConnection> {
    if (!userHub) {
        userHub = await buildHub("user");
        await startHub(userHub);
    }

    return userHub;
}

export async function startProjectHub(): Promise<HubConnection> {
    if (!projectHub) {
        projectHub = await buildHub("project");
        await startHub(projectHub);
    }

    return projectHub;
}

async function startHub(hub: HubConnection) {
    let tryConnect = async () => {
        try {
            if (hub?.state === "Disconnected") {
                await hub?.start();
            }

            return true;
        } catch {
            return false;
        }
    }
    
    if (await tryConnect()) {
        return;
    }
    
    let interval = setInterval(async () => {
        if (await tryConnect()) {
            clearInterval(interval);
        }
    }, 2500);
}