import {writable} from "svelte/store";
import type {HubConnection} from "@microsoft/signalr";

export const projectHub = writable<HubConnection | undefined>(undefined);
export const ticketsPerPage = 25;