<script lang="ts">
    import type {Project} from "../../gen/planeraClient";
    import {page} from "$app/stores";
    import ErrorText from "$lib/ErrorText.svelte";

    export let data: {
        projects: Project[],
        error: boolean,
    };

    $: path = $page.url.pathname
</script>

<div id="wrapper">
    <aside>
        <span class="label">Me</span>
        <div class="menu group">
            <a class="entry"
               href="/"
               class:selected={path === "/"}>
                Overview
            </a>
        </div>
        <span class="label">Projects</span>
        <div class="projects group">
            {#if data.error}
                <ErrorText value="Error loading projects." />
            {/if}
            {#each data?.projects ?? [] as project}
                {@const href = `/projects/${project.author.userName}/${project.slug}`}
                <a class="entry"
                   {href}
                    class:selected={path === href}>
                    <span class="logo">
                        {#if project.logo}
                            <img class="image" src={project.logo} alt="Project logo" />
                        {:else}
                            <span class="letter">{ project.name[0] }</span>
                        {/if}
                    </span>
                    <span class="name">{ project.name }</span>
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
        padding: 0.4em

        .label
            margin-top: 0.2em
            margin-left: 0.2em
            color: var(--text-gray)
            text-transform: uppercase
            font-weight: 600
            font-size: 0.8em

        .group
            display: flex
            flex-direction: column
            margin-bottom: 2em

        .entry
            display: flex
            align-items: center

            padding: var(--vertical-padding) var(--horizontal-padding)
            margin-top: 0.4em

            border-radius: var(--radius)
            color: black
            text-decoration: none
            cursor: pointer

            .logo
                display: flex
                align-items: center
                justify-content: center

                width: 1.35em
                height: 1.35em
                margin-left: -0.2em
                margin-right: 0.4em
                border-radius: 100%
                background-color: gray

                .letter
                    font-size: 0.75em
                    font-weight: 750
                    color: white

            &.selected
                cursor: default
                background-color: #dbeafe

            &:hover:not(&.selected)
                background-color: #e7e5e4

    main
        padding: var(--spacing)
</style>