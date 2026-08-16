<script lang="ts">
    interface Props {
        headers: Array<string>;
        children?: import('svelte').Snippet;
    }

    let { headers, children }: Props = $props();
</script>

<table>
    <thead>
    <tr>
        {#each headers as header}
            <th>{header}</th>
        {/each}
    </tr>
    </thead>
    <tbody>
        {@render children?.()}
    </tbody>
</table>

<style lang="sass">
    th
        position: sticky
        top: 0
        z-index: 1
        padding: var(--vertical-padding) var(--horizontal-padding)
        background-color: var(--background-secondary)
        border-top: 0
        font-weight: 600
        text-align: left
        
        // Hack to prevent the borders from disappearing when scrolling
        &::before, &::after
            position: absolute
            content: ''
            left: 0
            width: 100%
            background-color: var(--border-color)
            z-index: 2
        
        &::before
            top: 0
            height: calc(var(--border-width) / 2)
        
        &::after
            bottom: 0
            height: var(--border-width)
        
    table
        width: 100%
        border-collapse: collapse
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        border-top: 0
        
        :global(tr:first-child td)
            border-top: 0
            border-bottom: 0
</style>
