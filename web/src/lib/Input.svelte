<script lang="ts">
    import type {HTMLInputTypeAttribute} from "svelte/elements";

    export let type: HTMLInputTypeAttribute = "text";
    export let value: string = "";
    export let name: string;
    export let placeholder: string;
</script>

{#if type === "text"}
    <input type="text"
           bind:value={value}
           placeholder={placeholder}
           name={name} />
{:else if type === "password"}
    <input type="password"
           bind:value={value}
           placeholder={placeholder}
           name={name} />
{:else if type === "email"}
    <input type="email"
           bind:value={value}
           placeholder={placeholder}
           name={name} />
{/if}

<style lang="sass">
    input
        font-size: 1rem
        padding: calc(var(--vertical-padding) - var(--border-width)) calc(var(--horizontal-padding) - var(--border-width))
        border-radius: var(--radius)
        border: 0
        outline: var(--border)

        &:focus
            outline-width: 2px
            outline-color: var(--primary)
</style>