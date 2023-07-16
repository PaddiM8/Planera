<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";

    export let form;
</script>

<svelte:head>
    <title>Sign In - Planera</title>
</svelte:head>

<div id="content">
    <h1>Sign In</h1>

    <Form errors={form?.errors}>
        <Input name="username" placeholder="Username..." value={form?.username ?? ""} />
        <Input type="password" name="password" placeholder="Password..." />
        <Button value="Sign In" primary submit />
    </Form>
</div>

<style lang="sass">
    #content
        width: 100%
        max-width: 25rem
        margin-left: 50%
        margin-top: 50%
        transform: translate(-50%, -50%)
        padding-bottom: 20em
</style>