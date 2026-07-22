<script lang="ts">
    import TicketList from "$lib/components/ticket/TicketList.svelte";
    import {type ProjectDto, type TicketDto, TicketFilter, TicketSorting} from "../../gen/planeraClient";
    
    export let data: {
        project: ProjectDto,
        sorting: TicketSorting,
        filter: TicketFilter,
        tickets: TicketDto[],
    };
</script>

<svelte:head>
    <title>Overview - Planera</title>
</svelte:head>

<h1>Overview</h1>

<TicketList bind:sorting={data.sorting}
            bind:filter={data.filter}
            bind:tickets={data.tickets}
            isOverview/>
