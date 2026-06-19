import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
	plugins: [sveltekit()],
	resolve: {
		alias: {
			yjs: ("./node_modules/yjs/src/index.js"),
		},
	},
	server: {
		// Uses the port Aspire assigns, falling back to 5173 locally
		port: process.env.PORT ? parseInt(process.env.PORT) : 5173,
		strictPort: true,
	},
	ssr: {
		// Without this, there would be SSR errors in production builds
		noExternal: process.env.NODE_ENV === "development" ? [] : ["lexical"],
	},
});
