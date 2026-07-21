<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import CenteredLayout from "$lib/components/CenteredLayout.svelte";
    import {toast} from "$lib/toast";
    import {onMount} from "svelte";
    import type {AuthenticationInfo} from "../../../gen/planeraClient";
    import {goto} from "$app/navigation";
    import IconButton from "$lib/components/IconButton.svelte";
    import {Icon} from "svelte-hero-icons";

    export let form;
    export let data: {
        emailConfirmed: boolean;
        authenticationInfo: AuthenticationInfo | undefined;
    };
    
    let isEmailConfirmationFailure: boolean;
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

<CenteredLayout fitToContent={data.authenticationInfo?.passwordAuthenticationDisabled}>
    <div>
        <h1>Sign In</h1>

        {#if !data.authenticationInfo?.passwordAuthenticationDisabled}
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
        {/if}
    </div>
    {#if data?.authenticationInfo?.oidc}
        {#if !data.authenticationInfo?.passwordAuthenticationDisabled}
            <hr>
        {/if}
        <div class="oidc">
            <a class="oidc-link" href={import.meta.env.VITE_PUBLIC_API_URL.trimEnd("/") + "/auth/login/oidc"}>
                <button class="oidc-button">
                    {#if data.authenticationInfo?.oidc.providerIconUrl}
                    <span class="icon-container">
                        <img class="icon" src={data.authenticationInfo.oidc.providerIconUrl} alt="" />
                    </span>
                    {/if}
                    <span class="text">Sign In With {data.authenticationInfo.oidc.providerName}</span>
                </button>
            </a>
        </div>
    {/if}
</CenteredLayout>

<style lang="sass">
    .buttons
        display: flex

        a
            margin-right: auto
            text-decoration: none
            color: var(--primary)

            &:hover
                text-decoration: underline

            &:visited
                color: var(--primary)

    .resend-confirmation-email
        margin-top: -0.6em

    .oidc-link
        text-decoration: none

    .oidc-button
        display: flex
        align-items: center
        gap: 0.4em

        padding: var(--vertical-padding) var(--horizontal-padding)
        border: 0
        border-radius: var(--radius)

        background-color: var(--primary)
        color: var(--on-primary)
        font-size: 1rem
        font-weight: 600
        cursor: pointer
          
        &:hover
            background-color: var(--primary-hover)
      
        .icon-container
            min-width: 1em
            height: 1em
      
        .icon
            width: auto
            height: 100%
</style>