<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {PersonalAccessTokenMetadataDto} from "../../../../gen/planeraClient.js";
    import {invalidateAll} from "$app/navigation";
    import {dialog} from "$lib/dialog.js";
    import {toast} from "$lib/toast.js";
    import {userHub} from "../store.js";
    import Input from "$lib/components/form/Input.svelte";

    export let data: {
        personalAccessTokenMetadata: PersonalAccessTokenMetadataDto | null,
    };
    export let form;

    function afterSubmit(success: boolean) {
        if (success) {
            toast.info("Personal access token created. It will only be shown once.");
        } else {
            toast.error("Failed to create personal access token");
        }
    }

    async function revoke() {
        if (!await dialog.yesNo("Warning", "Are you sure you want to revoke your personal access token?")) {
            return;
        }

        try {
            await $userHub!.invoke("revokePersonalAccessToken");
            data.personalAccessTokenMetadata = null;
        } catch (ex) {
            console.log(ex);
            toast.error("Failed to revoke personal access token");
        }
    }

    function formatDate(date: Date) {
        const parts = new Intl.DateTimeFormat("en-GB", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            hourCycle: "h23",
        }).formatToParts(date);
        const lookup = Object.fromEntries(parts.map(p => [p.type, p.value]));

        return `${lookup.year}-${lookup.month}-${lookup.day} ${lookup.hour}:${lookup.minute}`;
    }
</script>

<svelte:head>
    <title>Token Settings - Planera</title>
</svelte:head>

<h1>Tokens</h1>

<h2>Personal Access Token</h2>
<section class="pat">
    {#if data.personalAccessTokenMetadata?.createdAtUtc}
        <div class="validity">
            <span class="label">Valid from</span>
            <span class="created-at">{formatDate(data.personalAccessTokenMetadata.createdAtUtc)}</span>
        </div>

        {#if form?.token}
            <Input value={form?.token} readonly />
        {:else}
            <Input type="password" value="--------------------------------" readonly />
        {/if}

        <Form action="?/createPersonalAccessToken" {afterSubmit}>
            <Button value="Regenerate" submit disabled={form?.token} />
        </Form>
        <Button value="Revoke" danger on:click={revoke} />
    {:else}
        <Form action="?/createPersonalAccessToken" {afterSubmit}>
            <Button value="Create New" primary submit />
        </Form>
    {/if}
</section>

<style lang="sass">
    section

    .pat
        display: flex
        align-items: center
        gap: 0.8em

    .validity
        display: flex
        flex-direction: column
        margin-right: auto

    .validity > span
        text-wrap: nowrap

    .label
        font-weight: 550
        margin-bottom: 0
</style>
