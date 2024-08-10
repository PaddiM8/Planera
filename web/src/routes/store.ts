import {writable} from "svelte/store";
import type {InterfaceTheme} from "../gen/planeraClient";

export const theme = writable<InterfaceTheme>();
