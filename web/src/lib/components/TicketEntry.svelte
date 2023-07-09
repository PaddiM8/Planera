<script lang="ts">
    import type {TicketDto} from "../../gen/planeraClient";
    import {priorityToName} from "$lib/priority";
    import UserIcon from "$lib/components/UserIcon.svelte";

    export let ticket: TicketDto;
</script>

<a class="ticket" href="/projects/{ticket.author.userName}/{ticket.projectSlug}/tickets/{ticket.id}">
    <div class="top">
        <h3 class="title">{ticket.title}</h3>
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
                    <UserIcon name={assignee.userName}
                              image={assignee.image}
                              type="user" />
                </span>
            {/each}
        </span>
    </div>
</a>

<style lang="sass">
    .ticket
        --ticket-background-rgb: var(--background-component-rgb)
        display: flex
        flex-direction: column
        padding: calc(var(--vertical-padding) * 1.5) calc(var(--horizontal-padding) * 1.5)
        margin-bottom: 0.4em
        border: var(--border)
        border-radius: var(--radius)
        color: var(--on-background)
        background-color: var(--background-component)
        text-decoration: none
        cursor: pointer

        &:hover
            --ticket-background-rgb: var(--hover-on-background-rgb)
            background-color: var(--hover-on-background)

    .top
        display: flex
        gap: 0.4em

    .id
        margin-left: auto

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

            $step1: rgba(var(--ticket-background-rgb), 0.85) 30%
            $step2: rgba(var(--ticket-background-rgb), 0.6) 75%
            background: linear-gradient(0deg, rgb(var(--ticket-background-rgb)), $step1, $step2, transparent)
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