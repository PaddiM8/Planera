<script lang="ts">
    import {createEventDispatcher} from "svelte";

    interface Props {
        value: string;
        primary?: boolean;
        submit?: boolean;
        danger?: boolean;
        disabled?: boolean;
    }

    let {
        value = $bindable(),
        primary = false,
        submit = false,
        danger = false,
        disabled = false
    }: Props = $props();

    const dispatch = createEventDispatcher();
    let element: HTMLElement = $state();

    function handleClick(e: any) {
        dispatch("click", e.details);
    }

    export function click() {
        element.click();
    }
</script>

{#if submit}
    <input type="submit"
           class:primary
           class:danger
           bind:value
           {disabled}
           bind:this={element} />
{:else}
    <button type="button"
            class:primary
            class:danger
            {disabled}
            onclick={handleClick}
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

        &.danger
            background-color: var(--red)
            color: var(--on-primary)

            &:hover
                background-color: var(--red-hover)

        &:hover
            background-color: var(--button-background-hover)
</style>
