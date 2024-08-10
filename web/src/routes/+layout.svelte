<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import ContextMenu from "$lib/components/ContextMenu.svelte";
    import ContextMenuEntry from "$lib/components/ContextMenuEntry.svelte";
    import {ArrowRightOnRectangle, Bars3, Cog, Icon} from "svelte-hero-icons";
    import Label from "$lib/components/GroupLabel.svelte";
    import Toast from "$lib/components/Toast.svelte";
    import YesNoDialog from "$lib/components/dialogs/YesNoDialog.svelte";
    import {InterfaceTheme, UserDto} from "../gen/planeraClient";
    import {getAvatarUrl} from "$lib/clients";
    import {user} from "./(main)/store";
    import {page} from "$app/stores";
    import {theme} from "./store"
    import {browser} from "$app/environment";
    import {onMount} from "svelte";

    export let data = {
        user: UserDto,
    };

    theme.subscribe(loadTheme);

    function loadTheme(interfaceTheme: InterfaceTheme | undefined) {
        if (interfaceTheme == undefined || !browser) {
            return;
        }

        document.head.insertAdjacentHTML("beforeend", getThemeLinkHtml(interfaceTheme));
    }

    function getThemeLinkHtml(interfaceTheme: InterfaceTheme) {
        let themeName = "light";
        if (interfaceTheme === InterfaceTheme.Dark) {
            themeName = "dark";
        }

        return `<link rel="stylesheet" href="/themes/${themeName}.css">`;
    }

    $: {
        if (data.user) {
            $user = data.user;
        }
    }

    let contextMenuTarget: HTMLElement | undefined;

    function openSidebar() {
        const sidebar = document.getElementById("sidebar");
        sidebar?.classList.add("open");
    }

    function handleUserClick(e) {
        contextMenuTarget = contextMenuTarget
            ? undefined
            : e.target;
    }
</script>

<svelte:head>
    {@html getThemeLinkHtml(data.user?.theme ?? InterfaceTheme.Light)}
</svelte:head>

<Toast />
<YesNoDialog />

<ContextMenu bind:target={contextMenuTarget}>
    <Label value="@{data.user?.username}" />
    <ContextMenuEntry name="Settings" href="/settings/general">
        <Icon src={Cog} />
    </ContextMenuEntry>
    <ContextMenuEntry name="Log Out" href="/logout" noPreload>
        <Icon src={ArrowRightOnRectangle} />
    </ContextMenuEntry>
</ContextMenu>

<div id="content">
    <header>
        <button class="sidebar-button" on:click={openSidebar}>
            <Icon src={Bars3} />
        </button>
        <a href="/" class="logo">
            <img src="/icon.svg" alt="logo" />
            <span>Planera</span>
        </a>
        <div class="items">
            {#if data?.user}
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <div class="user" on:click={handleUserClick}>
                    <UserIcon type="user"
                              name={data.user.username}
                              image={getAvatarUrl(data.user.avatarPath, "big")} />
                </div>
            {:else if $page.url.pathname === "/register"}
                <a href="/login" class="item">Sign In</a>
            {:else}
                <a href="/register" class="item">Register</a>
            {/if}
        </div>
    </header>

    <section class="page">
        <slot></slot>
    </section>
</div>

<style lang="sass">
    @use "@fontsource-variable/inter"
    @use "../values"

    :global(*)
        font-family: "Inter", sans-serif

    :global(body)
        width: 100%
        height: 100%

        padding: 0
        margin: 0
        color: var(--on-background)

    :global(h1)
        font-size: 2.1em
        margin-top: 0
        margin-bottom: 0.4em
        font-weight: 650

    :global(h2)
        font-size: 1.6em
        margin-top: 0.2em
        margin-bottom: 0.4em
        font-weight: 600

    :global(h3)
        font-size: 1.2em
        margin-top: 0.2em
        margin-bottom: 0.2em
        font-weight: 500

    :global(hr)
        border: 0
        border-bottom: var(--border)
        margin-top: 1.2em
        margin-bottom: 1.2em

    :global(button)
      border-radius: 0
      border: 0
      padding: 0
      background-color: inherit
      font-family: inherit

    #content
        display: grid
        grid-template-rows: auto 1fr
        height: 100vh
        max-height: 100vh
        background-color: var(--background-secondary)

    header
        display: flex
        align-items: center
        padding: var(--vertical-padding)
        background-color: var(--background)
        border-bottom: var(--border)

        .sidebar-button
            display: none
            width: 1.5em
            height: 1.5em
            background-color: transparent
            border: 0
            padding: 0
            cursor: pointer
            -webkit-tap-highlight-color: transparent

        .logo
            display: flex
            align-items: center
            font-size: 1.5em
            font-weight: 600
            color: var(--on-background)
            text-decoration: none
            margin: 0 0 0 0.4em

            img
                width: auto
                height: 1.1em
                margin-right: 0.4em

        .user
            position: relative
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

        .items
            margin-left: auto

        .item
            font-size: 1em
            font-weight: 500
            padding: 0.25em 0.4em
            border-radius: var(--radius)
            color: var(--on-background)
            text-decoration: none
            cursor: pointer

            &:hover
                background-color: var(--background-hover)

    .page
        display: flex
        max-width: 70em
        width: 100%
        height: 100%
        margin: 0 auto
        overflow: auto

    @media screen and (max-width: values.$max-width-for-hidden-sidebar)
        header .sidebar-button
            display: block
</style>