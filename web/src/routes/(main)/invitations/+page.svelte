<script lang="ts">
    import {Check, Icon, XMark} from "svelte-hero-icons";
    import type {ProjectDto} from "../../../gen/planeraClient";
    import {toast} from "$lib/toast";
    import {invitations} from "../store";
    import {userHub} from "../store";
    import IconButton from "$lib/components/IconButton.svelte";

    async function handleAccept(invitation: ProjectDto) {
        await $userHub!.invoke("acceptInvitation", invitation.id);
        invitations.update(value => value.filter(x => x != invitation));
        toast.info("Accepted invitation.");
    }

    async function handleDecline(invitation: ProjectDto) {
        await $userHub!.invoke("declineInvitation", invitation.id);
        invitations.update(value => value.filter(x => x != invitation));
    }
</script>

<svelte:head>
    <title>Invitations - Planera</title>
</svelte:head>

<h1>Invitations</h1>

<section class="invitation-list">
    {#if $invitations.length === 0}
        <span class="invitation empty">
            <span class="name">No invitations.</span>
        </span>
    {/if}
    {#each $invitations as invitation}
        <div class="invitation">
            <div class="about">
                <span class="name">
                    <a href="/projects/{invitation.author.username}/{invitation.slug}">
                        {invitation.name}
                    </a>
                </span>
                <span class="description">{invitation.description}</span>
            </div>
            <IconButton value="Decline"
                        icon={XMark}
                        color="red"
                        on:click={() => handleDecline(invitation)} />
            <IconButton value="Accept"
                        icon={Check}
                        color="green"
                        on:click={() => handleAccept(invitation)} />
        </div>
    {/each}
</section>

<style lang="sass">
    .invitation-list
        display: flex
        flex-direction: column
        width: 100%
        overflow-x: hidden

    .invitation
        display: flex
        align-items: center
        gap: 0.4em
        width: 100%
        padding: var(--vertical-padding) var(--horizontal-padding)
        border-top: var(--border)
        box-sizing: border-box

        &:last-child
            border-bottom: var(--border)

        &.empty .name
            color: var(--text-gray)

        a
            color: var(--on-background)
            text-decoration: none

            &:hover
                text-decoration: underline

        .about
            display: flex
            white-space: nowrap
            max-width: calc(100% - 10.75em)
            margin-right: auto

        .name
            font-weight: 450
            white-space: nowrap

        .description
            margin-left: 0.4em
            color: var(--text-gray)
            font-weight: 400
            white-space: nowrap
            overflow: hidden
            text-overflow: ellipsis
</style>