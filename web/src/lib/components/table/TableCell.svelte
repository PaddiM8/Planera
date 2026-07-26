<script>
</script>

<td>
    <div class="inner">
        <slot />
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
