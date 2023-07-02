<script lang="ts">
    import type {Project} from "../../gen/planeraClient";
    import {page} from "$app/stores";
    import ErrorText from "$lib/ErrorText.svelte";
    import UserIcon from "$lib/UserIcon.svelte";
    import Label from "$lib/Label.svelte";
    import {ListBullet, Icon} from "svelte-hero-icons";

    export let data: {
        projects: Project[],
        error: boolean,
    };

    $: path = $page.url.pathname
</script>

<div id="wrapper">
    <aside>
        <Label value="General" />
        <div class="menu group">
            <a class="entry"
               href="/"
               class:selected={path === "/"}>
                <span class="icon">
                    <Icon src={ListBullet} />
                </span>
                <span class="name">Overview</span>
            </a>
        </div>
        <Label value="Projects" />
        <div class="projects group">
            {#if data.error}
                <ErrorText value="Error loading projects." />
            {/if}
            {#each data?.projects ?? [] as project}
                {@const href = `/projects/${project.author.userName}/${project.slug}`}
                <a class="entry"
                   {href}
                    class:selected={path === href}>
                    <span class="icon">
                        <UserIcon type="project" name="{project.name}" />
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

        .group
            display: flex
            flex-direction: column
            margin-bottom: 1em

        .entry
            display: flex
            align-items: center
            gap: 0.4em

            padding: var(--vertical-padding) var(--horizontal-padding)
            margin-top: 0.4em

            border-radius: var(--radius)
            color: black
            text-decoration: none
            cursor: pointer

            &.selected
                cursor: default
                background-color: #dbeafe

            &:hover:not(&.selected)
                background-color: #e7e5e4

            .icon
                width: 1.5em
                height: 1.5em

    main
        padding: var(--spacing)
</style>