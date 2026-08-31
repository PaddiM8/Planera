<script lang="ts">
    import TicketList from "$lib/components/ticket/TicketList.svelte";
    import {type ProjectDto, type TicketDto, TicketFilter, TicketSorting} from "../../gen/planeraClient";
    
    interface Props {
        data: {
            sorting: TicketSorting,
            filter: TicketFilter,
            tickets: TicketDto[],
        };
    }
    
    let { data }: Props = $props();

    let sorting = $derived(data.sorting);
    let filter = $derived(data.filter);
    let tickets = $derived(data.tickets);
</script>

<svelte:head>
    <title>Overview - Planera</title>
</svelte:head>

<h1>Overview</h1>

<TicketList bind:sorting={sorting}
            bind:filter={filter}
            bind:tickets={tickets}
            isOverview/>
