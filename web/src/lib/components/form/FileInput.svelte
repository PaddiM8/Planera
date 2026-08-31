<script lang="ts">
    import Button from "$lib/components/form/Button.svelte";
    import type {ChangeEventHandler} from "svelte/elements";

    interface Props {
        text?: string;
        name?: string | undefined;
        accept?: string[] | undefined;
        onchange?: ChangeEventHandler<HTMLInputElement>;
    }

    let {
        text = $bindable("Browse"),
        name = undefined, 
        accept = undefined,
        onchange = undefined,
    }: Props = $props();

    let inputElement: HTMLInputElement = $state()!;
</script>

<input type="file"
       name={name}
       accept={accept?.join(",")}
       bind:this={inputElement}
       {onchange} />
<Button bind:value={text} onclick={() => inputElement?.click()} />

<style lang="sass">
    input
        display: block
        width: 0
</style>