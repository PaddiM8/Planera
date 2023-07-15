import {writable} from "svelte/store";
import type {ProjectDto, UserDto} from "../../gen/planeraClient";
import type {HubConnection} from "@microsoft/signalr";

export const userHub = writable<HubConnection | undefined>(undefined);

export const invitations = writable<ProjectDto[]>([]);

export const participants = writable<UserDto[]>([]);
