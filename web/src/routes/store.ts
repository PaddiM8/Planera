import {writable} from "svelte/store";
import type {InterfaceTheme} from "../gen/planeraClient";
import type {InsertImageDialog} from "@paddim8/svelte-lexical";

export const theme = writable<InterfaceTheme>();
export const lastMenuIndex = writable<number>();
export const insertImageDialog = writable<InsertImageDialog>();
