<script lang="ts">
    import Button from "$lib/components/form/Button.svelte";
    import {createEventDispatcher} from "svelte";

    export let text: string = "Browse";
    export let name: string | undefined = undefined;
    export let accept: string[] | undefined = undefined;

    let inputElement: HTMLInputElement;
    const dispatcher = createEventDispatcher();

    function handleChange(e) {
        dispatcher("change", e);
    }
</script>

<input type="file"
       name={name}
       accept={accept?.join(",")}
       bind:this={inputElement}
       on:change={handleChange} />
<Button bind:value={text} on:click={inputElement?.click()} />

<style lang="sass">
    input
        display: block
        width: 0
</style>