<script lang="ts">
    import type {TicketDto} from "../../gen/planeraClient";
    import {priorityToName} from "$lib/priority";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {Check, Icon, Minus, XMark} from "svelte-hero-icons";
    import {TicketStatus} from "../../gen/planeraClient";
    import {projectHub} from "../../routes/(main)/projects/[user]/[slug]/store";
    import {getAvatarUrl} from "$lib/clients";

    export let ticket: TicketDto;

    async function setStatus(status: TicketStatus) {
        await $projectHub?.invoke(
            "setTicketStatus",
            ticket.projectId,
            ticket.id,
            status
        );
    }
</script>

<div class="ticket" class:has-status={ticket.status}>
    <div class="top">
        {#if ticket.status === TicketStatus.Done}
            <div class="status done" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Check} />
            </div>
        {:else if ticket.status === TicketStatus.Inactive}
            <div class="status inactive" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Minus} />
            </div>
        {:else if ticket.status === TicketStatus.Closed}
            <div class="status closed" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={XMark} />
            </div>
        {/if}
        <a href="/projects/{ticket.author.username}/{ticket.projectSlug}/tickets/{ticket.id}">
            <h3 class="title">{ticket.title}</h3>
        </a>
        <div class="status-buttons">
            <button class="status-button close" on:click={() => setStatus(TicketStatus.Closed)}>
                <span class="icon"><Icon src={XMark} /></span>
                <span class="text">Close</span>
            </button>
            <button class="status-button inactive" on:click={() => setStatus(TicketStatus.Inactive)}>
                <span class="icon"><Icon src={Minus} /></span>
                <span class="text">Inactive</span>
            </button>
            <button class="status-button done" on:click={() => setStatus(TicketStatus.Done)}>
                <span class="icon"><Icon src={Check} /></span>
                <span class="text">Done</span>
            </button>
        </div>
        <h3 class="id">{ticket.id}</h3>
    </div>
    <span class="description">{@html ticket.description}</span>
    <div class="bottom">
        <span class="priority priority-{priorityToName(ticket.priority)}">
            {priorityToName(ticket.priority)}
        </span>
            <span class="assignees">
            {#each ticket.assignees as assignee}
                <span class="assignee">
                    <UserIcon name={assignee.username}
                              image={getAvatarUrl(assignee.avatarPath, "small")}
                              type="user" />
                </span>
            {/each}
        </span>
    </div>
</div>

<style lang="sass">
    .ticket
        display: flex
        flex-direction: column
        padding: calc(var(--vertical-padding) * 1.5) calc(var(--horizontal-padding) * 1.5)
        padding-top: calc(var(--vertical-padding) * 1.5 - 0.3em)
        margin-bottom: 0.4em
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

    .top
        display: flex
        align-items: center
        gap: 0.4em
        margin-bottom: 0.2em

        a
            color: var(--on-background)
            text-decoration: none

    .status
        display: block
        height: 1.2em
        margin-right: -0.2em
        cursor: pointer

        &.done
            color: green

        &.inactive
            color: cornflowerblue

        &.closed
            color: red

    .title
        margin-top: 0
        margin-bottom: 0

        &:hover
            text-decoration: underline

    .ticket:not(:hover) .status-button, .ticket.has-status .status-button
        visibility: hidden

    .status-buttons
        display: flex
        gap: 0.4em
        margin-left: auto
        margin-right: 0.4em

    .status-button
        display: flex
        align-items: center
        background-color: transparent
        border: 0
        padding: 0.3em 0.4em
        border-radius: var(--radius)
        font-family: inherit
        font-weight: 500
        cursor: pointer

        &:hover
            background-color: var(--background-hover)

        &.done .icon
            color: green

        &.inactive .icon
            color: cornflowerblue

        &.close .icon
            color: red

        .icon
            display: block
            margin-right: 0.1em
            height: 1.4em

    .id
        margin-top: 0
        margin-bottom: 0

        &::before
            content: '#'

    .description
        position: relative
        max-height: 7em
        margin-bottom: 0.4em
        overflow: hidden

        &::before
            $fade-height: 3em
            position: absolute
            content: ''
            top: calc(7em - #{$fade-height})
            left: 0
            width: 100%
            height: $fade-height

            $step1: rgba(var(--component-background-rgb), 0.85) 30%
            $step2: rgba(var(--component-background-rgb), 0.6) 75%
            background: linear-gradient(0deg, var(--component-background), $step1, $step2, transparent)
            z-index: 999999

        :global(br)
            display: none

    .bottom
        display: flex
        gap: 0.4em

    .priority
        display: flex
        align-items: center
        padding: 0.2em 0.4em
        border-radius: var(--radius)

        font-size: 0.7em
        font-weight: 500
        color: white

        &:global(.priority-None)
            display: none

        &:global(.priority-Low)
            background-color: var(--low)

        &:global(.priority-Normal)
            background-color: var(--normal)

        &:global(.priority-High)
            background-color: var(--high)

        &:global(.priority-Severe)
            background-color: var(--severe)

    .assignees
        display: flex
        gap: 0.25em

        .assignee
            height: 1.125em
</style>