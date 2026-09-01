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
    
    export async function getBase64Url(): Promise<string | null> {
        if (!inputElement.files) {
            return null;
        }

        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(inputElement.files![0]);
            reader.onload = () => resolve(reader.result as string);
            reader.onerror = (error) => reject(error);
        });
    }
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