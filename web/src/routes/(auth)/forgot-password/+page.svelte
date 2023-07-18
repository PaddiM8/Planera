<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import CenteredLayout from "$lib/components/CenteredLayout.svelte";
    import {onMount} from "svelte";
    import {toast} from "$lib/toast";

    export let form;
    export let data;

    function handleSubmit(success: boolean) {
        if (success) {
            toast.info("Sent password reset email.");
        }
    }
</script>

<svelte:head>
    <title>Forgot Password - Planera</title>
</svelte:head>

<CenteredLayout>
    <h1>Forgot Password</h1>

    <Form problem={form?.problem} afterSubmit={handleSubmit}>
        <Input name="username" placeholder="Username..." />
        <Button value="Reset" primary submit />
    </Form>
</CenteredLayout>
