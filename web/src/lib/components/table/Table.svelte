<script lang="ts">
    export let headers: Array<string>;
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
        <slot />
    </tbody>
</table>

<style lang="sass">
    th
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        font-weight: 600
        text-align: left
        
    table
      width: 100%
      border-collapse: collapse
      padding: var(--vertical-padding) var(--horizontal-padding)
      border: var(--border)
</style>
