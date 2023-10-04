<script lang="ts">
    import {createEventDispatcher, onMount} from "svelte";

    export let choices: string[];
    export let selectedValue: string | undefined = undefined;
    export let defaultChoice: string | undefined = undefined;
    export let name: string | undefined = undefined;

    const dispatcher = createEventDispatcher();

    onMount(() => {
        if (!selectedValue) {
            reset();
        }
    });

    export function reset() {
        selectedValue = "";

        if (defaultChoice) {
            selectedValue = defaultChoice;
        }
    }

    function handleChange(e: Event, value: string) {
        if ((e.target as HTMLInputElement).checked) {
            selectedValue = value;
            dispatcher("change", value);
        }
    }
</script>

<span class="multi-button">
    {#each choices as choice}
        <span class="button">
            <input type="radio"
                   id="choice-{choice}"
                   value={choice}
                   name={name}
                   bind:group={selectedValue}
                   on:change={e => handleChange(e, choice)} />
            <label for="choice-{choice}">{choice}</label>
        </span>
    {/each}
</span>

<style lang="sass">
    .multi-button
        display: flex
        width: fit-content
        border: var(--border)
        border-radius: var(--radius)

    .button
        position: relative

        &:first-of-type label
            border-top-left-radius: var(--radius)
            border-bottom-left-radius: var(--radius)

        &:last-child label
            border-top-right-radius: var(--radius)
            border-bottom-right-radius: var(--radius)
            border-right: 0

    input[type="radio"]
        position: absolute
        top: 0
        left: 0
        z-index: -1

    input[type="radio"]:checked + label
        background-color: var(--background-hover)

    input[type="radio"]:focus-visible + label
        position: relative
        outline: 2px solid var(--blue)
        z-index: 999

    .button:not(:last-of-type) input[type="radio"]:focus + label
        border-right: var(--border)

    input[type="radio"] + label
        display: block
        content: ""
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: 0
        border-right: var(--border)
        background-color: var(--component-background)
        color: var(--on-background)
        font-weight: 450
        user-select: none
        cursor: pointer
        -webkit-tap-highlight-color: transparent

        &:hover:not(input[type="radio"]:checked + label)
            background-color: var(--background-hover)
</style>