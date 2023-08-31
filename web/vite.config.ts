import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
	plugins: [sveltekit()],
	resolve: {
		alias: {
			yjs: ("./node_modules/yjs/src/index.js"),
		},
	},
	ssr: {
		// Without this, there would be SSR errors in production builds
		noExternal: process.env.NODE_ENV === "development" ? [] : ["lexical"],
	},
});
