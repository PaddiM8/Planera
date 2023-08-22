import {type HubConnection, HubConnectionBuilder} from "@microsoft/signalr";

/*export enum TicketFilter {
    All = "All",
    Open = "Open",
    Closed = "Closed",
    Inactive = "Inactive",
    Done = "Done",
}
export enum TicketSorting {
    Newest = "Newest",
    Oldest = "Oldest",
    HighestPriority = "Highest Priority",
    LowestPriority = "Lowest Priority",
}*/

async function buildHub(name: string): Promise<HubConnection> {
    return new HubConnectionBuilder()
        .withUrl(`${import.meta.env.VITE_SERVER_URL}/hubs/${name}`)
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
