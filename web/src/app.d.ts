// See https://kit.svelte.dev/docs/types#app
// for information about these interfaces
import type {ProjectDto} from "./gen/planeraClient";

declare global {
	namespace App {
		// interface Error {}
		interface Locals {
			project: ProjectDto,
		}
		//interface PageData {}
		// interface Platform {}
	}
}

export {};
