<script lang="ts">
    import type {Project} from "../../gen/planeraClient";
    import {page} from "$app/stores";

    export let data: {
        projects: Array<Project>,
    };

    function isActive(path) {
        return $page.url.pathname.startsWith(path);
    }
</script>

<div id="wrapper">
    <aside>
        <div class="menu group">
            <a class="entry"
               href="/"
               class:selected={isActive("/")}>
                Dashboard
            </a>
        </div>
        <span class="label">Projects</span>
        <div class="projects group">
            {#each data?.projects ?? [] as project}
                {@const href = "/projects/{project.author}/{project.slug}"}
                <a class="entry"
                   {href}
                    class:selected={isActive(href)}>
                    { project.name }
                </a>
            {/each}
        </div>
    </aside>
    <main>
        <slot></slot>
    </main>
</div>

<style lang="sass">
    #wrapper
        display: grid
        grid-template-columns: 20em auto
        width: 100%

        border-left: var(--border)
        border-right: var(--border)

    aside
        display: flex
        flex-direction: column
        border-right: var(--border)

        .label
            margin-left: 0.6em
            color: var(--text-gray)
            text-transform: uppercase
            font-weight: 600
            font-size: 0.9em

        .group
            display: flex
            flex-direction: column
            margin-bottom: 2em

        .entry
            padding: var(--spacing)
            border-bottom: var(--border)
            color: black
            text-decoration: none
            cursor: pointer

            &.selected
                cursor: default
                background-color: var(--selected-on-background)

            &:hover:not(&.selected)
                background-color: var(--hover-on-background)
</style>