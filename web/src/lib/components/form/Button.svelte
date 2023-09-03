<script lang="ts">
    import {createEventDispatcher} from "svelte";

    export let value: string;
    export let primary = false;
    export let submit = false;
    export let disabled = false;

    const dispatch = createEventDispatcher();
    let element: HTMLElement;

    function handleClick(e) {
        dispatch("click", e.details);
    }

    export function click() {
        element.click();
    }
</script>

{#if submit}
    <input type="submit"
           class:primary
           bind:value
           {disabled}
           bind:this={element} />
{:else}
    <button type="button"
            class:primary
            {disabled}
            on:click={handleClick}
            bind:this={element}>{value}</button>
{/if}

<style lang="sass">
    input[type="submit"], button
        align-self: flex-end
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: 0
        border-radius: var(--radius)

        background-color: var(--button-background)
        color: var(--on-button-background)
        font-size: 1rem
        font-weight: 600
        cursor: pointer

        &[disabled], &.primary[disabled]
            background-color: var(--button-background-disabled)
            cursor: default

            &:hover
                background-color: var(--button-background-disabled)

        &.primary
            background-color: var(--primary)
            color: var(--on-primary)

            &:hover
                background-color: var(--primary-hover)

        &:hover
            background-color: var(--button-background-hover)
</style>