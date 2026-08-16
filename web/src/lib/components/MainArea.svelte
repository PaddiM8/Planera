<script lang="ts">
    interface Props {
        children?: import('svelte').Snippet;
    }

    let { children }: Props = $props();
</script>

<main id="main-area">
    {@render children?.()}
</main>

<style lang="sass">
    main
        padding: calc(var(--spacing) * 1.5)
        overflow-x: hidden
        overflow-y: auto
        max-height: 100%
</style>
