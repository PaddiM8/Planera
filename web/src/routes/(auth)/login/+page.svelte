<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import CenteredLayout from "$lib/components/CenteredLayout.svelte";
    import {toast} from "$lib/toast";
    import {onMount} from "svelte";
    import {browser} from "$app/environment";

    export let form;
    export let data;

    let isEmailConfirmationFailure;
    let usernameValue: string;

    onMount(() => {
        if (data?.emailConfirmed) {
            toast.info("Email confirmed successfully.", 4000);
        }
    });

    $: {
        isEmailConfirmationFailure = form?.problem &&
            Object.keys(form.problem).some(x => x === "email");
    }

    async function sendConfirmationMail() {
        const result = await fetch(`/send-confirmation-email?username=${usernameValue}`);
        if (result.ok) {
            toast.info("Sent confirmation email.", 7000);
        } else {
            toast.error("Failed to send confirmation email.");
        }
    }
</script>

<svelte:head>
    <title>Sign In - Planera</title>
</svelte:head>

<CenteredLayout>
    <h1>Sign In</h1>

    <Form problem={form?.problem}>
        {#if isEmailConfirmationFailure}
            <a href="./"
               class="resend-confirmation-email"
               on:click={sendConfirmationMail}>Resend Confirmation Email</a>
        {/if}
        <Input name="username" placeholder="Username..." bind:value={usernameValue} />
        <Input type="password" name="password" placeholder="Password..." />
        <div class="buttons">
            <a href="/forgot-password">Forgot password?</a>
            <Button value="Sign In" primary submit />
        </div>
    </Form>
</CenteredLayout>

<style lang="sass">
    .buttons
        display: flex

        a
            margin-right: auto

    .resend-confirmation-email
        margin-top: -0.6em
</style>