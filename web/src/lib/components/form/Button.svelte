<script lang="ts">
    export let value: string;
    export let primary: boolean;
    export let submit: boolean;
</script>

{#if submit}
    <input type="submit" class:primary={primary} bind:value={value} />
{:else}
    <button class:primary={primary}>{value}</button>
{/if}

<style lang="sass">
    input[type="submit"], button
        align-self: flex-end
        padding: var(--vertical-padding) var(--horizontal-padding)
        background-color: var(--button-background)
        color: var(--on-button-background)
        font-size: 1rem
        font-weight: 600

        border: 0
        border-radius: var(--radius)
        cursor: pointer

        &.primary
            background-color: var(--primary)
            color: var(--on-primary)

            &:hover
                background-color: var(--primary-hover)

        &:hover
            background-color: var(--button-background-hover)
</style>