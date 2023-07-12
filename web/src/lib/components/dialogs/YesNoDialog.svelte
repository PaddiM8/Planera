<script lang="ts">
    import Dialog from "$lib/components/dialogs/Dialog.svelte";
    import Button from "$lib/components/form/Button.svelte";
</script>

<Dialog id="yes-no-dialog" title="">
    <span class="message"></span>
    <div class="buttons">
        <Button value="No" />
        <Button value="Yes" primary />
    </div>
</Dialog>

<style lang="sass">
</style>