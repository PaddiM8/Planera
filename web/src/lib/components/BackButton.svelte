<script lang="ts">
    import {ArrowLongLeft, Icon} from "svelte-hero-icons";

    export let placeName: string;
    export let href: string | undefined;
</script>

<a {href}>
    <button on:click>
    <span class="icon">
        <Icon src={ArrowLongLeft} />
    </span>
        <span class="text">Back to {placeName}</span>
    </button>
</a>

<style lang="sass">
    a
        text-decoration: none

    button
        display: flex
        align-items: center
        gap: 0.4em
        padding: calc(var(--vertical-padding) / 1.5) calc(var(--horizontal-padding) / 1.5)
        margin-bottom: 0.4em
        border: 0
        border-radius: var(--radius)
        background-color: transparent
        font-size: 1em
        font-weight: 500
        cursor: pointer

        &:hover
            background-color: var(--background-hover)

        .icon
            width: 1.2em
            height: 1.2em
</style>