<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import CenteredLayout from "$lib/components/CenteredLayout.svelte";
    import {toast} from "$lib/toast";
    import {browser} from "$app/environment";

    export let form;
    export let data;

    $: {
        if (browser && form?.mailSent) {
            toast.info("Confirmation email sent.", 7500);
        }
    }
</script>

<svelte:head>
    <title>Register - Planera</title>
</svelte:head>

<CenteredLayout>
    <h1>Register</h1>

    <Form errors={form?.errors}>
        <Input name="username" placeholder="Username..." />
        <Input name="email" placeholder="Email Address..." />
        <Input type="password" name="password" placeholder="Password..." />
        <Input type="password" name="confirmedPassword" placeholder="Repeat Password..." />
        <Button value="Register" primary submit />
    </Form>
</CenteredLayout>
