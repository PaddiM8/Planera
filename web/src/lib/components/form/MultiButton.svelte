<script lang="ts">
    import {createEventDispatcher, onMount} from "svelte";

    export let name: string;
    export let choices: string[];
    export let defaultChoice: string | undefined = undefined;
    export let selectedIndex: number | undefined;

    let defaultInput;
    let element: HTMLElement;
    const dispatcher = createEventDispatcher();

    onMount(() => {
        if (defaultChoice && !selectedIndex) {
            selectedIndex = choices.indexOf(defaultChoice);
        }
    });

    export function reset() {
        defaultInput.checked = true;
    }

    function handleChange(e, index: number) {
        if (e.target.checked) {
            selectedIndex = index;
            dispatcher("change", index);
        }
    }
</script>

<span class="multi-button" bind:this={element}>
    {#each choices as choice, i}
        {#if selectedIndex === i}
            <input type="radio"
                   id="choice-{choice}"
                   value={choice}
                   name={name}
                   checked />
        {:else}
            <input type="radio"
                   id="choice-{choice}"
                   value={choice}
                   name={name}
                   on:change={e => handleChange(e, i)} />
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
        background-color: var(--button-background-selected)

    input[type="radio"] + label
        display: block
        content: 'hello'
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: 0
        border-right: 1px solid var(--button-background-hover)
        background-color: var(--button-background)
        color: var(--on-button-background)
        font-weight: 450
        user-select: none
        cursor: pointer

        &:hover:not(input[type="radio"]:checked + label)
            background-color: var(--button-background-hover)

        &:first-of-type
            border-top-left-radius: var(--radius)
            border-bottom-left-radius: var(--radius)

        &:last-child
            border-top-right-radius: var(--radius)
            border-bottom-right-radius: var(--radius)
            border-right: 0
</style>