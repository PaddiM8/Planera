import {type HubConnection, HubConnectionBuilder} from "@microsoft/signalr";

async function buildHub(name: string): Promise<HubConnection> {
    return new HubConnectionBuilder()
        .withUrl(`http://localhost:5065/hubs/${name}`)
        .withAutomaticReconnect()
        .build();
}

export async function startUserHub(): Promise<HubConnection> {
    const hub = await buildHub("user");
    await startHub(hub);

    return hub;
}

export async function startProjectHub(): Promise<HubConnection> {
    const hub = await buildHub("project");
    await startHub(hub);

    return hub;
}

async function startHub(hub: HubConnection) {
    try {
        await hub.start();
    } catch (err) {
        console.log(err);
        setTimeout(() => startHub(hub), 5000);
    }
}
