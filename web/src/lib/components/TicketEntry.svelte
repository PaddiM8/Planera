<script lang="ts">
    import type {TicketDto} from "../../gen/planeraClient";
    import {priorityToName} from "$lib/priority";

    export let ticket: TicketDto;
</script>

<div class="ticket">
    <div class="top">
        <h3 class="title">{ticket.title}</h3>
        <h3 class="id">{ticket.id}</h3>
    </div>
    <span class="description">{@html ticket.description}</span>
    <span class="priority">{priorityToName(ticket.priority)}</span>
</div>

<style lang="sass">
    .ticket
        display: flex
        flex-direction: column
        padding: calc(var(--vertical-padding) * 1.5) calc(var(--horizontal-padding) * 1.5)
        margin-bottom: 0.4em
        border: var(--border)
        border-radius: var(--radius)
        background-color: white

    .top
        display: flex
        gap: 0.4em

    .id
        margin-left: auto

        &::before
            content: '#'

    .description
        margin-bottom: 0.4em

    .priority
        font-size: 0.8em
        font-weight: 500
</style>