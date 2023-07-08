<script lang="ts">
    import {onMount} from "svelte";

    export let name: string;
    export let choices: string[];
    export let defaultChoice: string | undefined = undefined;

    let defaultInput;

    export function reset() {
        defaultInput.checked = true;
    }
</script>

<span class="multi-button">
    {#each choices as choice}
        {#if choice === defaultChoice}
            <input type="radio"
                   id="choice-{choice}"
                   value={choice}
                   name={name}
                   bind:this={defaultInput}
                   checked />
        {:else}
            <input type="radio"
                   id="choice-{choice}"
                   value={choice}
                   name={name} />
        {/if}
        <label for="choice-{choice}">{choice}</label>
    {/each}
</span>

<style lang="sass">
    .multi-button
        display: flex

    input[type="radio"]
        display: none

    input[type="radio"]:checked + label
        background-color: var(--button-selected-background)

    input[type="radio"] + label
        display: block
        content: 'hello'
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: 0
        border-right: 1px solid var(--button-hover-background)
        background-color: var(--button-background)
        color: var(--on-button-background)
        font-weight: 450
        user-select: none
        cursor: pointer

        &:hover:not(input[type="radio"]:checked + label)
            background-color: var(--button-hover-background)

        &:first-of-type
            border-top-left-radius: var(--radius)
            border-bottom-left-radius: var(--radius)

        &:last-child
            border-top-right-radius: var(--radius)
            border-bottom-right-radius: var(--radius)
            border-right: 0
</style>