<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {Check, Icon, Minus, Plus, XMark} from "svelte-hero-icons";
    import {TicketStatus, TicketDto} from "../../../gen/planeraClient";
    import {projectHub} from "../../../routes/(main)/projects/[user]/[slug]/store";
    import {getAvatarUrl} from "$lib/clients";
    import PriorityLabel from "$lib/components/ticket/PriorityLabel.svelte";
    import IconButton from "$lib/components/IconButton.svelte";
    import {goto} from "$app/navigation";
    import {toast} from "$lib/toast";
    import {user} from "../../../routes/(main)/store";

    export let ticket: TicketDto;

    let showTouchOverlay = false;
    let preventTouch = false;

    $: ticketUrl = `/projects/${ticket?.author.username}/${ticket?.projectSlug}/tickets/${ticket?.id}`;

    async function setStatus(status: TicketStatus) {
        if (preventTouch) {
            return;
        }

        closeTouchOverlay();
        await $projectHub?.invoke(
            "setTicketStatus",
            ticket.projectId,
            ticket.id,
            status
        );
    }

    async function assignToMe() {
        try {
            await $projectHub!.invoke(
                "addTicketAssignee",
                ticket.projectId,
                ticket.id,
                $user.id
            );

            toast.info("Added assignee successfully.");
        } catch {
            toast.error("Failed to add assignee.");
            ticket.assignees = ticket.assignees.filter(x => x.id !== $user.id);
        }
    }

    function openTouchOverlay() {
        if (preventTouch) {
            return;
        }

        if (window["closeTouchOverlay"]) {
            window["closeTouchOverlay"]();
        }

        window["closeTouchOverlay"] = closeTouchOverlay;
        showTouchOverlay = true;
        preventTouch = true;
        setTimeout(() => {
            preventTouch = false;
        }, 175);
    }

    export function closeTouchOverlay() {
        if (preventTouch) {
            return;
        }

        showTouchOverlay = false;
        preventTouch = true;
        setTimeout(() => {
            preventTouch = false;
        }, 175);
    }
</script>

<div class="ticket" class:has-status={ticket.status}>
    <div class="touch-overlay" on:touchstart={openTouchOverlay}>
        <div class="menu" class:shown={showTouchOverlay}>
            <span class="row">
                {#if ticket.status === TicketStatus.None}
                    <button class="item" on:touchstart={() => setStatus(TicketStatus.Done)}>
                        <span class="icon done">
                            <Icon src={Check} />
                        </span>
                        <span class="name">Done</span>
                    </button>
                    <button class="item" on:touchstart={() => setStatus(TicketStatus.Inactive)}>
                        <span class="icon inactive">
                            <Icon src={Minus} />
                        </span>
                        <span class="name">Inactive</span>
                    </button>
                    <button class="item" on:touchstart={() => setStatus(TicketStatus.Closed)}>
                        <span class="icon close">
                            <Icon src={XMark} />
                        </span>
                        <span class="name">Close</span>
                    </button>
                {:else}
                    <button class="item" on:touchstart={() => setStatus(TicketStatus.None)}>
                        <span class="name">Clear Status</span>
                    </button>
                {/if}
            </span>
            <span class="row">
                <button class="item" on:touchstart={() => !preventTouch && goto(ticketUrl)}>
                    <span class="name">Open</span>
                </button>
                <button class="item" on:touchstart={closeTouchOverlay}>
                    <span class="name">Back</span>
                </button>
            </span>
        </div>
    </div>
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
        <a href={ticketUrl}>
            <h3 class="title">{ticket.title}</h3>
        </a>
        <div class="status-buttons">
            <IconButton value="Close"
                        icon={XMark}
                        color="red"
                        on:click={() => setStatus(TicketStatus.Closed)} />
            <IconButton value="Inactive"
                        icon={Minus}
                        color="blue"
                        on:click={() => setStatus(TicketStatus.Inactive)} />
            <IconButton value="Done"
                        icon={Check}
                        color="green"
                        on:click={() => setStatus(TicketStatus.Done)} />
        </div>
        <h3 class="id">{ticket.id}</h3>
    </div>
    <span class="description">{@html ticket.description}</span>
    <div class="bottom">
        <PriorityLabel bind:priority={ticket.priority} />
        <span class="assignees">
            {#each ticket.assignees as assignee}
                <span class="assignee">
                    <UserIcon name={assignee.username}
                              image={getAvatarUrl(assignee.avatarPath, "small")}
                              type="user" />
                </span>
            {/each}
            <button class="assignee add-button" on:click={assignToMe}>
                <Icon src={Plus} />
            </button>
        </span>
    </div>
</div>

<style lang="sass">
    .ticket
        position: relative
        display: flex
        flex-direction: column
        padding: calc(var(--vertical-padding) * 1.5) calc(var(--horizontal-padding) * 1.5)
        padding-top: calc(var(--vertical-padding) * 1.5 - 0.1em)
        margin-bottom: 0.4em
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

    .touch-overlay
        position: absolute
        display: none
        top: 0
        left: 0
        width: 100%
        height: 100%
        box-sizing: border-box
        border-radius: var(--radius)
        overflow: hidden
        z-index: 999

        & .menu
            display: flex
            flex-direction: column
            width: 100%
            height: 100%
            background-color: var(--component-background)

            .row
                display: flex
                flex-grow: 1
                flex-basis: 50%
                border-bottom: var(--border)

                &:last-child
                    border-bottom: 0

            .item
                display: flex
                flex-direction: column
                align-items: center
                justify-content: center
                flex-grow: 1
                flex-basis: 50%
                background-color: var(--component-background)
                border: 0
                border-right: var(--border)
                font-weight: 500

                &:last-child
                    border-right: 0

                &:hover
                    background-color: var(--background-hover)

                .icon
                    width: 1.5em

                    &.done
                        color: var(--green)

                    &.inactive
                        color: var(--blue)

                    &.close
                        color: var(--red)

                .name
                    font-size: 1.1em

            &:not(.shown)
                display: none

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
        min-width: 1.2em
        margin-right: -0.2em
        cursor: pointer

        &.done
            color: var(--green)

        &.inactive
            color: var(--blue)

        &.closed
            color: var(--red)

    .title
        margin-top: 0
        margin-bottom: 0

        &:hover
            text-decoration: underline

    .ticket:not(:hover) .status-buttons, .ticket.has-status .status-buttons, .ticket:not(:hover) .add-button
        visibility: hidden

    .status-buttons
        display: flex
        gap: 0.4em
        margin-left: auto
        margin-right: 0.4em

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
            $fade-height: 1.25em
            position: absolute
            content: ''
            top: calc(7em - #{$fade-height})
            left: 0
            width: 100%
            height: $fade-height

            $step1: rgba(var(--component-background-rgb), 0.85) 30%
            $step2: rgba(var(--component-background-rgb), 0.65) 50%
            $step3: rgba(var(--component-background-rgb), 0.4) 80%
            background: linear-gradient(0deg, var(--component-background), $step1, $step2, transparent)
            z-index: 500

        :global(br)
            display: none

    .bottom
        display: flex
        align-items: center
        gap: 0.4em

    .assignees
        display: flex
        gap: 0.25em

        .add-button
            display: flex
            align-items: center
            justify-content: center
            width: 1.375em
            height: 1.375em
            padding: 0
            aspect-ratio: 1 / 1
            text-align: center
            border-radius: 100%
            border: var(--border)
            background-color: var(--background)
            font-size: 1em
            cursor: pointer

            &:hover
                background-color: var(--background-hover)

        :global(.add-button > *)
            width: 0.75em
            height: 0.75em

    @media (hover: none)
        .status-buttons
            display: none

        .id
            margin-left: auto

        .touch-overlay
            display: block
</style>