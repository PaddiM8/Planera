<script lang="ts">
    import {priorityToName} from "$lib/priority";
    import type {TicketPriority} from "../../../gen/planeraClient";

    export let priority: TicketPriority;
</script>

<span class="priority priority-{priorityToName(priority)}">
    {priorityToName(priority)}
</span>

<style lang="sass">
    .priority
        display: flex
        align-items: center
        padding: 0.2em 0.4em
        border-radius: var(--radius)
        width: fit-content

        font-size: 0.7em
        font-weight: 500
        color: white
        cursor: default

        &:global(.priority-None)
            background-color: var(--none)

        &:global(.priority-Low)
            background-color: var(--low)

        &:global(.priority-Normal)
            background-color: var(--normal)

        &:global(.priority-High)
            background-color: var(--high)

        &:global(.priority-Severe)
            background-color: var(--severe)

</style>