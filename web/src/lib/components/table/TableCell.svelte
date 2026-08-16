<script lang="ts">
    interface Props {
        children?: import('svelte').Snippet;
    }

    let { children }: Props = $props();
</script>

<td>
    <div class="inner">
        {@render children?.()}
    </div>
</td>

<style lang="sass">
    td
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        
    .inner
        display: flex
        gap: 0.8em
</style>
