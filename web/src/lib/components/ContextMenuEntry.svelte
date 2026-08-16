<script lang="ts">
    interface Props {
        name: string;
        href?: string | undefined;
        noPreload?: boolean;
        children?: import('svelte').Snippet;
    }

    let {
        name,
        href = undefined,
        noPreload = false,
        children
    }: Props = $props();
</script>

<svelte:element this={href ? "a" : "span"}
                href={href}
                class="entry"
                data-sveltekit-preload-data={noPreload ? "tap" : ""}>
    <span class="icon">
        {@render children?.()}
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