<script lang="ts">
    import Dialog from "$lib/components/dialogs/Dialog.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {InsertImage} from "@paddim8/svelte-lexical";
    import ImagePicker from "$lib/components/form/ImagePicker.svelte";
    
    let dialog: Dialog = $state()!;
    let imagePicker: ImagePicker = $state()!;
    let currentEditor: any;
    
    export function open(editor: any) {
        currentEditor = editor;
        dialog.open();
    }

    export function close() {
        dialog.close();
        currentEditor = null;
    }
    
    async function handleSubmit() {
        const url = await imagePicker.getBase64Url();
        const payload = {
            altText: "",
            src: url!,
        };
        console.log(payload);
        InsertImage(currentEditor, payload);
        imagePicker.clear();
        close();
    }
</script>

<Dialog bind:this={dialog} title="Insert Image">
    <div class="body">
        <ImagePicker bind:this={imagePicker} />
    </div>
    <div class="buttons">
        <Button value="Cancel" onclick={close} />
        <Button value="Insert" primary onclick={handleSubmit} />
    </div>
</Dialog>

<style lang="sass">
    .buttons
        margin-top: 0.4em
</style>
