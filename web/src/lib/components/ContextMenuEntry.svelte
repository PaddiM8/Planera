<script lang="ts">
    export let name: string;
    export let href: string | undefined = undefined;
    export let noPreload: boolean = false;
</script>

<svelte:element this={href ? "a" : "span"}
                href={href}
                class="entry"
                data-sveltekit-preload-data={noPreload ? "tap" : ""}>
    <span class="icon">
        <slot />
    </span>
    <span class="name">{ name }</span>
</svelte:element>

<style lang="sass">
    .entry
        display: flex
        align-items: center
        gap: 0.4em

        padding: 0.4em
        border-radius: var(--radius)
        color: var(--on-background)
        text-decoration: none
        font-weight: 500
        white-space: nowrap
        cursor: pointer
        -webkit-tap-highlight-color: transparent

        .icon
            width: 1.5em
            height: 1.5em

        &:hover
            background-color: var(--background-hover)
</style>