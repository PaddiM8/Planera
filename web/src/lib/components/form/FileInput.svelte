<script lang="ts">
    import Button from "$lib/components/form/Button.svelte";
    import {createEventDispatcher} from "svelte";

    interface Props {
        text?: string;
        name?: string | undefined;
        accept?: string[] | undefined;
    }

    let { text = $bindable("Browse"), name = undefined, accept = undefined }: Props = $props();

    let inputElement: HTMLInputElement = $state();
    const dispatcher = createEventDispatcher();

    function handleChange(e) {
        dispatcher("change", e);
    }
</script>

<input type="file"
       name={name}
       accept={accept?.join(",")}
       bind:this={inputElement}
       onchange={handleChange} />
<Button bind:value={text} on:click={inputElement?.click()} />

<style lang="sass">
    input
        display: block
        width: 0
</style>