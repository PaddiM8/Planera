<script lang="ts">
    import type {HTMLInputTypeAttribute} from "svelte/elements";
    import {createEventDispatcher} from "svelte";
    import Button from "$lib/components/form/Button.svelte";

    export let type: HTMLInputTypeAttribute = "text";
    export let value: string = "";
    export let name: string = "";
    export let placeholder: string;
    export let label: string | undefined = undefined;
    export let submitButton: Button | undefined = undefined;

    const dispatch = createEventDispatcher();

    function handleInput(e) {
        dispatch("input", e.detail);
    }

    function handleKeyDown(e) {
        if (e.key === "Enter" && submitButton) {
            submitButton.click();
        }
    }
</script>

{#if label}
    <label for="input-{name}">{label}</label>
{/if}

{#if type === "text"}
    <input type="text"
           id="input-{name}"
           bind:value={value}
           placeholder={placeholder}
           {name}
           on:input={handleInput}
           on:keydown={handleKeyDown} />
{:else if type === "password"}
    <input type="password"
           id="input-{name}"
           bind:value={value}
           {placeholder}
           {name}
           on:input={handleInput}
           on:keydown={handleKeyDown} />
{:else if type === "email"}
    <input type="email"
           id="input-{name}"
           bind:value={value}
           placeholder={placeholder}
           {name}
           on:input={handleInput}
           on:keydown={handleKeyDown} />
{/if}

<style lang="sass">
    label
        margin-bottom: -0.4em
        font-size: 1em
        font-weight: 450

    input
        font-size: 1rem
        padding: var(--vertical-padding) var(--horizontal-padding)
        border-radius: var(--radius)
        border: 0
        background-color: var(--component-background)
        outline: var(--border)
        box-sizing: border-box

        &:focus
            outline-width: 2px
            outline-color: var(--primary)
</style>