<script lang="ts">
    import {priorityToName} from "$lib/priority";
    import type {TicketPriority} from "../../../gen/planeraClient";
    import {TicketStatus} from "../../../gen/planeraClient";

    interface Props {
        priority: TicketPriority;
        status: TicketStatus | undefined;
    }

    let {
        priority = $bindable(),
        status = $bindable()
    }: Props = $props();
</script>

<span class="priority priority-{priorityToName(priority)}"
      class:active={status !== TicketStatus.Done && status !== TicketStatus.Closed}>
    {priorityToName(priority)}
</span>

<style lang="sass">
    .priority
        display: flex
        align-items: center
        padding: 0.2em 0.4em
        border-radius: var(--radius)
        width: fit-content

        background-color: var(--none)
        font-size: 0.7em
        font-weight: 500
        color: white
        cursor: default

        &.active
            &:global(.priority-Low)
                color: var(--on-low)
                background-color: var(--low)

            &:global(.priority-Normal)
                color: var(--on-normal)
                background-color: var(--normal)

            &:global(.priority-High)
                color: var(--on-high)
                background-color: var(--high)

            &:global(.priority-Severe)
                color: var(--on-severe)
                background-color: var(--severe)

</style>