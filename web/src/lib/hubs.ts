import {type HubConnection, HubConnectionBuilder, HubConnectionState} from "@microsoft/signalr";

let projectHub: HubConnection;

export async function startProjectHub(): Promise<HubConnection> {
    if (!projectHub) {
        const hub = new HubConnectionBuilder()
            .withUrl("http://localhost:5065/hubs/project")
            .withAutomaticReconnect()
            .build();
        await startHub(hub);
        projectHub = hub;
    }

    return projectHub;
}

export function getProjectHub(): HubConnection {
    return projectHub;
}

async function startHub(hub: HubConnection) {
    try {
        await hub.start();
        console.assert(hub.state === HubConnectionState.Connected);
    } catch (err) {
        console.assert(hub.state === HubConnectionState.Disconnected);
        console.log(err);
        setTimeout(() => startHub(hub), 5000);
    }
}
