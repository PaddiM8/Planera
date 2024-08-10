import {writable} from "svelte/store";
import type {HubConnection} from "@microsoft/signalr";

export const userHub = writable<HubConnection | undefined>(undefined);