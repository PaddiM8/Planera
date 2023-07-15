<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import ContextMenu from "$lib/components/ContextMenu.svelte";
    import ContextMenuEntry from "$lib/components/ContextMenuEntry.svelte";
    import { Icon, Cog, ArrowRightOnRectangle } from "svelte-hero-icons";
    import Label from "$lib/components/form/Label.svelte";
    import Toast from "$lib/components/Toast.svelte";
    import YesNoDialog from "$lib/components/dialogs/YesNoDialog.svelte";
    import {UserDto} from "../gen/planeraClient";

    export let data = {
        user: UserDto,
    };

    let contextMenuTarget: HTMLElement | undefined;

    function handleUserClick(e) {
        contextMenuTarget = contextMenuTarget
            ? undefined
            : e.target;
    }
</script>

<Toast />
<YesNoDialog />

<ContextMenu bind:target={contextMenuTarget}>
    <Label value="@{data.user?.userName}" />
    <ContextMenuEntry name="User Settings" href="/settings">
        <Icon src={Cog} />
    </ContextMenuEntry>
    <ContextMenuEntry name="Log Out" href="/logout" noPreload>
        <Icon src={ArrowRightOnRectangle} />
    </ContextMenuEntry>
</ContextMenu>

<div id="content">
    <header>
        <span class="logo">Planera</span>
        {#if data?.user}
            <div class="user" on:click={handleUserClick}>
                <UserIcon type="user" name={ data.user.userName } />
            </div>
        {/if}
    </header>

    <section class="page">
        <slot></slot>
    </section>
</div>

<style lang="sass">
    @use "@fontsource-variable/inter"

    :global(body)
        width: 100%
        height: 100%

        padding: 0
        margin: 0
        font-family: "Inter", sans-serif

    :global(:root)
        --background: #fafaf9
        --on-background: black
        --on-background-inactive: #404040
        --background-hover: #e7e5e4
        --background-selected: #bbdefb
        --background-secondary: #f5f5f4
        --background-secondary-hover: #d4d4d8
        --component-background-rgb: 255, 255, 255
        --component-background: rgb(var(--component-background-rgb))
        --primary: #1d4ed8
        --on-primary: #fafafa
        --primary-hover: #2563eb
        --button-background: #d4d4d4
        --on-button-background: #0a0a0a
        --button-background-hover: #bbbbbb
        --button-background-selected: #a8a8a8
        --border-width: 1px
        --border: var(--border-width) solid #d1d5db
        --text-gray: #757575
        --spacing: 0.8rem
        --vertical-padding: 0.55rem
        --horizontal-padding: 0.7rem
        --radius: 0.375rem
        --low: #2563eb
        --normal: #059669
        --high: #f97316
        --severe: #ef4444

    :global(h1)
        font-size: 2.1em
        margin-top: 0
        margin-bottom: 0.2em
        font-weight: 650

    :global(h2)
        font-size: 1.6em
        margin-top: 0.2em
        margin-bottom: 0.6em
        font-weight: 600

    :global(h3)
        font-size: 1.2em
        margin-top: 0.2em
        margin-bottom: 0.2em
        font-weight: 500

    :global(hr)
        border: 0
        border-top: var(--border)
        margin-top: 1.2em
        margin-bottom: 1.2em

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
        max-width: 75.2em
        width: 100%
        height: 100%
        margin: 0 auto
</style>