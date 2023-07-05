<script lang="ts">
    import PageData = App.PageData;
    import UserIcon from "$lib/UserIcon.svelte";
    import ContextMenu from "$lib/ContextMenu.svelte";
    import ContextMenuEntry from "$lib/ContextMenuEntry.svelte";
    import { Icon, Cog, ArrowRightOnRectangle } from "svelte-hero-icons";
    import Label from "$lib/Label.svelte";

    export let data: PageData;

    let contextMenuTarget: HTMLElement | undefined;

    function handleUserClick(e) {
        contextMenuTarget = contextMenuTarget
            ? undefined
            : e.target;
    }
</script>

<ContextMenu bind:target={contextMenuTarget}>
    <Label value="@{data.user?.username}" />
    <ContextMenuEntry name="User Settings" href="/settings">
        <Icon src={Cog} />
    </ContextMenuEntry>
    <ContextMenuEntry name="Log Out" href="/logout">
        <Icon src={ArrowRightOnRectangle} />
    </ContextMenuEntry>
</ContextMenu>

<div id="content">
    <header>
        <span class="logo">Planera</span>
        {#if data?.user}
            <div class="user" on:click={handleUserClick}>
                <UserIcon type="user" name={ data.user.username } />
            </div>
        {/if}
    </header>

    <section class="page">
        <slot></slot>
    </section>
</div>

<style lang="sass">
    @import "@fontsource-variable/inter"

    :global(body)
        width: 100%
        height: 100%

        padding: 0
        margin: 0
        font-family: "Inter", sans-serif

    :global(:root)
        --background: #fafaf9
        --on-background: black
        --background-secondary: #f5f5f4
        --primary: #1d4ed8
        --on-primary: #fafafa
        --primary-hover: #2563eb
        --button-background: #cbd5e1
        --on-button-background: #0a0a0a
        --hover-on-background: #d3d3d3
        --selected-on-background: #bbdefb
        --border-width: 1px
        --border: var(--border-width) solid #d1d5db
        --text-gray: #757575
        --spacing: 0.8rem
        --vertical-padding: 0.55rem
        --horizontal-padding: 0.7rem
        --radius: 0.375rem

    :global(h1)
        font-size: 2.1em
        margin-top: 0
        margin-bottom: 0.2em
        font-weight: 650

    :global(h2)
        font-size: 1.7em
        margin-top: 0
        margin-bottom: 0.2em
        font-weight: 600

    :global(h3)
        font-size: 1.2em
        margin-top: 0
        margin-bottom: 0.2em
        font-weight: 500

    #content
        display: grid
        grid-template-rows: auto 1fr
        min-height: 100vh
        background-color: var(--background)

    header
        display: flex
        align-items: center
        padding: var(--vertical-padding)
        border-bottom: var(--border)

        .logo
            font-size: 1.5em
            font-weight: 600
            margin: 0 0 0 0.4em

        .user
            position: relative
            margin-left: auto
            border-radius: 100%
            width: 1.35em
            height: 1.35em
            font-size: 1.6em
            cursor: pointer

            &:hover::before
                position: absolute
                content: ''
                top: 0
                bottom: 0
                width: 100%
                height: 100%
                border-radius: 100%
                background-color: rgba(0, 0, 0, 0.1)

    .page
        display: flex
        max-width: 80em
        height: 100%

        margin-left: 50%
        transform: translateX(-50%)
</style>