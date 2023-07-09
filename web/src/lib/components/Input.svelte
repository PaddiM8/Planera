<script lang="ts">
    import type {HTMLInputTypeAttribute} from "svelte/elements";
    import {createEventDispatcher} from "svelte";

    export let type: HTMLInputTypeAttribute = "text";
    export let value: string = "";
    export let name: string = "";
    export let placeholder: string;
    export let contenteditable: boolean = false;

    const dispatch = createEventDispatcher();

    function handleInput(e) {
        dispatch("input", e.detail);
    }
</script>

{#if type === "text"}
    <input type="text"
           bind:value={value}
           placeholder={placeholder}
           {contenteditable}
           {name}
           on:input={handleInput} />
{:else if type === "password"}
    <input type="password"
           bind:value={value}
           {placeholder}
           {contenteditable}
           {name}
           on:input={handleInput} />
{:else if type === "email"}
    <input type="email"
           bind:value={value}
           placeholder={placeholder}
           {contenteditable}
           {name}
           on:input={handleInput} />
{/if}

<style lang="sass">
    input
        font-size: 1rem
        padding: var(--vertical-padding) var(--horizontal-padding)
        border-radius: var(--radius)
        border: 0
        background-color: var(--background-component)
        outline: var(--border)
        box-sizing: border-box

        &:focus
            outline-width: 2px
            outline-color: var(--primary)
</style>