<script lang="ts">
    import type {HTMLInputTypeAttribute} from "svelte/elements";
    import {createEventDispatcher} from "svelte";
    import Button from "$lib/components/form/Button.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";

    export let type: HTMLInputTypeAttribute = "text";
    export let value: string = "";
    export let name: string = "";
    export let placeholder: string;
    export let label: string | undefined = undefined;
    export let submitButton: Button | undefined = undefined;

    let wrapperElement: HTMLElement;

    export function focus() {
        (wrapperElement.firstElementChild as HTMLInputElement).focus();
    }

    function handleKeyDown(e) {
        if (e.key === "Enter" && submitButton) {
            submitButton.click();
        }
    }
</script>

<div class="wrapper" bind:this={wrapperElement}>
    {#if label}
        <FormLabel forId="input-{name}" value={label} />
    {/if}

    {#if type === "text"}
        <input type="text"
               id="input-{name}"
               bind:value={value}
               placeholder={placeholder}
               {name}
               on:input
               on:keydown={handleKeyDown} />
    {:else if type === "password"}
        <input type="password"
               id="input-{name}"
               bind:value={value}
               {placeholder}
               {name}
               on:input
               on:keydown={handleKeyDown} />
    {:else if type === "email"}
        <input type="email"
               id="input-{name}"
               bind:value={value}
               placeholder={placeholder}
               {name}
               on:input
               on:keydown={handleKeyDown} />
    {/if}
</div>

<style lang="sass">
    .wrapper
        width: 100%

    input
        display: block
        width: 100%
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