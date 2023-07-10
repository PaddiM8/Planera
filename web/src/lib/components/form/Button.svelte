<script lang="ts">
    import {createEventDispatcher} from "svelte";

    export let value: string;
    export let primary: boolean = false;
    export let submit: boolean = false;

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
           class:primary={primary}
           bind:value={value}
           bind:this={element} />
{:else}
    <button class:primary={primary}
            on:click={handleClick}
            bind:this={element}>{value}</button>
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