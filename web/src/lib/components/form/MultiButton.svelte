<script lang="ts">
    import {createEventDispatcher, onMount} from "svelte";

    export let choices: string[] = [];
    export let choiceValues: string[] | undefined = undefined;
    export let backgroundColors: string[] | undefined = undefined;
    export let foregroundColors: string[] | undefined = undefined;
    export let selectedValue: string | undefined = undefined;
    export let defaultValue: string | undefined = undefined;
    export let disabled: boolean = false;
    export let name: string | undefined = undefined;
    export let yesNo: boolean | undefined = undefined;
    
    const dispatcher = createEventDispatcher();
    
    if (yesNo) {
        choices = ["No", "Yes"];
        backgroundColors = ["var(--severe)", "var(--normal)"]
        foregroundColors = ["var(--on-severe)", "var(--on-normal)"]
        choiceValues = ["false", "true"]
    }

    onMount(() => {
        if (!selectedValue) {
            reset();
        }
    });

    export function reset() {
        selectedValue = "";

        if (defaultValue) {
            selectedValue = defaultValue;
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
    {#each choices as choice, i}
        {@const value = choiceValues ? choiceValues[i] : choice}
        {@const backgroundColor = backgroundColors ? backgroundColors[i] : undefined}
        {@const foregroundColor = foregroundColors ? foregroundColors[i] : undefined}
        <span class="button">
            <input type="radio"
                   id="choice-{name}-{value.replace(' ', '-')}"
                   value={value}
                   name={name}
                   disabled={disabled}
                   bind:group={selectedValue}
                   on:change={e => handleChange(e, value)} />
            <label for="choice-{name}-{value.replace(' ', '-')}"
                   style="{backgroundColor ? `background-color: ${backgroundColor};` : ''} {foregroundColor ? `color: ${foregroundColor};` : ''}">
                {choice}
            </label>
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

    input
        position: absolute
        top: 0
        left: 0
        z-index: -1
        
    input[disabled] + label
        cursor: default !important
        background-color: var(--button-background-disabled) !important
        color: var(--text-gray) !important
      
    input:checked + label
      color: var(--on-background)
      background-color: var(--background-selected)
      
    input:not([disabled]):not(:checked) + label
      color: var(--on-background) !important
      background-color: var(--component-background) !important

    input:focus-visible + label
        position: relative
        outline: 2px solid var(--blue)
        z-index: 999

    .button:not(:last-of-type) input:focus + label
        border-right: var(--border)

    input + label
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

    input:not([disabled]):hover:not(input:checked + label)
        background-color: var(--background-hover) !important
</style>