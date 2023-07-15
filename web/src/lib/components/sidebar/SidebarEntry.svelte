<script lang="ts">
    import {Cog, Icon} from "svelte-hero-icons";
    import {page} from "$app/stores";
    import {invitations} from "../../../routes/(main)/store";

    export let src: string;
    export let value: string;
    export let unreadCount = 0;
    export let settingsSrc: string | undefined = undefined;

    $: path = $page.url.pathname
</script>

<a class="entry"
   href={src}
   class:selected={path === src}>
    <span class="icon">
        <slot />
    </span>
    <span class="name">{value}</span>
    {#if unreadCount > 0}
        <span class="unread-count">{$invitations.length}</span>
    {/if}
    {#if settingsSrc}
        <a class="settings" href={settingsSrc}>
            <Icon src={Cog} />
        </a>
    {/if}
</a>

<style lang="sass">
    .entry
        display: flex
        align-items: center
        gap: 0.4em

        padding: var(--vertical-padding) var(--horizontal-padding)
        margin-top: 0.2em

        border-radius: var(--radius)
        color: black
        text-decoration: none
        font-weight: 425
        cursor: pointer

        &.selected
            cursor: default

        &:hover, &.selected
            background-color: var(--background-hover)

            .settings
                display: block

        .icon
            width: 1.5em
            height: 1.5em

        .unread-count
            $size: 1.3em
            margin-left: auto
            width: $size
            height: $size
            line-height: $size
            text-align: center

            background-color: crimson
            font-size: 0.7em
            font-weight: 550
            color: white
            border-radius: 100%

    .settings
        display: none
        margin-left: auto
        width: 1.5em
        height: 1.5em
        color: var(--on-background-inactive)
        text-decoration: none

        &:hover
            color: var(--on-background)
</style>