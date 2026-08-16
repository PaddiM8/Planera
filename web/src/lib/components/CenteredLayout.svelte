<script lang="ts">
    interface Props {
        fitToContent?: boolean;
        children?: import('svelte').Snippet;
    }

    let { fitToContent = false, children }: Props = $props();
</script>

<div id="content" style="width: {fitToContent ? 'auto' : '100%'}">
    {@render children?.()}
</div>

<style lang="sass">
    #content
        max-width: 25rem
        padding: 1.5em 2em
        margin: auto
        transform: translateY(-5em)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--background)

    @media screen and (max-width: 580px)
        :global(.page)
            background-color: var(--background)

        #content
            max-width: none
            border: 0
            border-radius: 0
            transform: none
            background-color: var(--background)
</style>
