<script lang="ts">
    import type {HTMLInputTypeAttribute} from "svelte/elements";
    import {createEventDispatcher, getContext} from "svelte";
    import Button from "$lib/components/form/Button.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";

    export let value: string = "";
    export let name: string = "";
    export let label: string | undefined = undefined;
    export let time: boolean = false;

    let wrapperElement: HTMLElement;

    export function focus() {
        (wrapperElement.firstElementChild as HTMLInputElement).focus();
    }
    
    function handleDateChange(event: Event) {
        if (time) {
            const target = event.target as HTMLInputElement;
            value = target.value + "T00:00"
        }
    }
</script>

<div class="wrapper" bind:this={wrapperElement}>
    {#if label}
        <FormLabel forId="input-{name}" value={label} />
    {/if}

    {#if time}
        {#if value}
            <input type="datetime-local"
                   id="input-{name}"
                   lang="{getContext('locale')}"
                   bind:value={value}
                   on:input
                   {name} />
        {:else}
            <input type="date"
                   id="input-{name}"
                   on:input
                   on:change={handleDateChange}
                   {name} />
        {/if}
    {:else}
        <input type="date"
               id="input-{name}"
               bind:value={value}
               on:input
               {name} />
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
        color: var(--on-background)
        background-color: var(--component-background)
        outline: var(--border)
        box-sizing: border-box
        color-scheme: var(--color-scheme)
        letter-spacing: -0.75px

        &:focus
            outline-width: 2px
            outline-color: var(--primary)
</style>
