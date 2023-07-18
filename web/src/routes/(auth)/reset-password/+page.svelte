<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import CenteredLayout from "$lib/components/CenteredLayout.svelte";

    export let form;
</script>

<svelte:head>
    <title>Reset Password - Planera</title>
</svelte:head>

<CenteredLayout>
    <h1>Reset Password</h1>

    <Form problem={form?.problem}>
        <Input type="password" name="newPassword" placeholder="New Password..." />
        <Input type="password" name="confirmedPassword" placeholder="Repeat Password..." />
        <Button value="Reset" primary submit />
    </Form>
</CenteredLayout>
