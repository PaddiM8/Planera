<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";

    export let form;
</script>

<svelte:head>
    <title>Register - Planera</title>
</svelte:head>

<div id="content">
    <h1>Register</h1>

    <Form errors={form?.errors}>
        <Input name="username" placeholder="Username..." />
        <Input name="email" placeholder="Email Address..." />
        <Input type="password" name="password" placeholder="Password..." />
        <Input type="password" name="confirmedPassword" placeholder="Repeat Password..." />
        <Button value="Register" primary submit />
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

    .buttons
        display: flex

        a
            margin-right: auto
</style>
