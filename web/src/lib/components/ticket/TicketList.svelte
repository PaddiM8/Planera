<script lang="ts">
    import Input from "$lib/components/form/Input.svelte";
    import TicketEntry from "$lib/components/ticket/TicketEntry.svelte";
    import {onDestroy, onMount} from "svelte";
    import {toast} from "$lib/toast";
    import Select from "$lib/components/form/Select.svelte";
    import {makeImagePathsAbsolute} from "$lib/paths";
    import {sanitizeHtml} from "$lib/formatting";
    import {getKeyFromValue} from "$lib/util.js";
    import {projectHub, ticketsPerPage} from "../../../routes/(main)/projects/[user]/[slug]/store";
    import {
        ProjectDto,
        TicketDto,
        TicketFilter,
        TicketQueryResult,
        TicketSorting,
        TicketStatus
    } from "../../../gen/planeraClient";
	import { userHub } from "../../../routes/(main)/store";

    export let project: ProjectDto | undefined = undefined;
    export let sorting: TicketSorting;
    export let filter: TicketFilter;
    export let tickets: Array<TicketDto>;
    export let isOverview: boolean = false;
    
    export function partialReset() {
        searchQuery = "";

        if (project?.allTicketsCount !== undefined && project?.openTicketsCount !== undefined) {
            refreshFilterMap();
        }
    }
 
    let searchQuery: string;
    const filterMap: { [key: string]: TicketFilter } = {};
    let filterKeys: string[] = [];
    let sortingString: string;
    let filterString: string;

    const sortingMap: { [key: string]: TicketSorting } = {
        "Newest": TicketSorting.Newest,
        "Oldest": TicketSorting.Oldest,
        "Highest Priority": TicketSorting.HighestPriority,
        "Lowest Priority": TicketSorting.LowestPriority,
    };
    let queryTimeout: NodeJS.Timeout;
    let reachedEndOfTickets = false;
    let isLoadingMore = false;

    $: updateSortingWithoutQuerying(sorting, filter);
    $: refreshFilterMap();

    function refreshFilterMap() {
        for (const key in filterMap) {
            delete filterMap[key];
        }

        if (project) {
            filterMap[`All (${project.allTicketsCount})`] = TicketFilter.All;
            filterMap[`Open (${project.openTicketsCount})`] = TicketFilter.Open;
            filterMap[`Closed (${project.closedTicketsCount})`] = TicketFilter.Closed;
            filterMap[`Inactive (${project.inactiveTicketsCount})`] = TicketFilter.Inactive;
            filterMap[`Done (${project.doneTicketsCount})`] = TicketFilter.Done;
            filterMap[`Assigned to Me (${project.assignedToMeCount})`] = TicketFilter.AssignedToMe;
        } else {
            filterMap[`All`] = TicketFilter.All;
            filterMap[`Open`] = TicketFilter.Open;
            filterMap[`Closed`] = TicketFilter.Closed;
            filterMap[`Inactive`] = TicketFilter.Inactive;
            filterMap[`Done`] = TicketFilter.Done;
            filterMap[`Assigned to Me`] = TicketFilter.AssignedToMe;
        }

        filterKeys = Object.keys(filterMap);

        // Since the keys changed, the currently selected filter value needs to change
        // as well
        if (filter) {
            setFilter(filterString?.split(" ").at(0) ?? "");
        }
    }

    function setFilter(name: string) {
        filterString = filterKeys.find(key => key.startsWith(name))
            ?? filterKeys[0];
    }

    function updateSortingWithoutQuerying(newSorting?: TicketSorting, newFilter?: TicketFilter) {
        const newSortingString = getKeyFromValue(sortingMap, newSorting ?? TicketSorting.Newest)!;
        const newFilterString = getKeyFromValue(filterMap, newFilter ?? TicketFilter.All)!;
        if (sorting  == undefined || newSortingString !== sortingString || filter == undefined || newFilterString !== filterString) {
            sortingString = newSortingString;
            filterString = newFilterString;
        }
    }

    $: if ($projectHub) {
        $projectHub.off("onUpdateTicket", onUpdateTicket);
        $projectHub.on("onUpdateTicket", onUpdateTicket);
    }

    onDestroy(() => {
        $projectHub?.off("onUpdateTicket", onUpdateTicket);
    });

    onMount(async () => {
        const mainArea = document.getElementById("main-area") as HTMLElement;
        mainArea.onscroll = e => {
            const target = e.target as HTMLElement;
            if (!isLoadingMore && target.scrollTop + target.clientHeight >= target.scrollHeight - 100) {
                loadMore();
            }
        }
    });

    async function loadMore() {
        if (reachedEndOfTickets) {
            return;
        }
        
        if (project) {
            if (!$projectHub || $projectHub.state !== "Connected") {
                return;
            }
        } else {
            if (!$userHub || $userHub.state !== "Connected") {
                return;
            }
        }

        isLoadingMore = true;
        let queryResult: TicketQueryResult | undefined;
        if (project) {
            queryResult = await $projectHub!.invoke(
                "queryTickets",
                project.author?.username,
                project.slug,
                tickets.length,
                ticketsPerPage,
                searchQuery,
                sortingMap[sortingString],
                filterMap[filterString],
            );
        } else {
            queryResult = await $userHub!.invoke(
                "queryTickets",
                tickets.length,
                ticketsPerPage,
                searchQuery,
                sortingMap[sortingString],
                filterMap[filterString],
            );
        }
        
        if (!queryResult?.tickets) {
            return;
        }

        for (const ticket of queryResult.tickets) {
            ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description));
        }

        let combinedTickets = [...tickets, ...queryResult.tickets];
        // De-duplicate
        combinedTickets = Array.from(
            new Map(combinedTickets.map(x => [x.id, x])).values()
        );
        tickets = combinedTickets;

        if (queryResult.tickets.length === 0) {
            reachedEndOfTickets = true;
        }

        isLoadingMore = false;
    }

    function onUpdateTicket(projectId: string, ticketId: number, newFields: TicketDto) {
        const index = tickets.findIndex(x => x.id === ticketId);
        if (index !== -1) {
            for (const [key, value] of Object.entries(newFields)) {
                const ticket = (tickets as any)[index];
                if (key === "status") {
                    updateTicketCounts(value, ticket.status);
                }

                ticket[key] = value;
            }
        }
    }

    function updateTicketCounts(newStatus: TicketStatus, oldStatus: TicketStatus) {
        if (!project) {
            return;
        }

        if (project.openTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.None) {
                project.openTicketsCount--;
            } else if (newStatus === TicketStatus.None) {
                project.openTicketsCount++;
            }
        }

        if (project.closedTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Closed) {
                project.closedTicketsCount--;
            } else if (newStatus === TicketStatus.Closed) {
                project.closedTicketsCount++;
            }
        }

        if (project.inactiveTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Inactive) {
                project.inactiveTicketsCount--;
            } else if (newStatus === TicketStatus.Inactive) {
                project.inactiveTicketsCount++;
            }
        }

        if (project.doneTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Done) {
                project.doneTicketsCount--;
            } else if (newStatus === TicketStatus.Done) {
                project.doneTicketsCount++;
            }
        }

        refreshFilterMap();
    }

    function query() {
        reachedEndOfTickets = false;
        const newTimeout = setTimeout(async () => {
            clearTimeout(queryTimeout);
            queryTimeout = newTimeout;

            try {
                let queryResult: TicketQueryResult | undefined;
                if (project) {
                    queryResult = await $projectHub!.invoke(
                        "queryTickets",
                        project.author?.username,
                        project.slug,
                        0,
                        ticketsPerPage,
                        searchQuery,
                        sortingMap[sortingString],
                        filterMap[filterString],
                    );
                } else {
                    queryResult = await $userHub!.invoke(
                        "queryTickets",
                        0,
                        ticketsPerPage,
                        searchQuery,
                        sortingMap[sortingString],
                        filterMap[filterString],
                    );
                }
                
                if (!queryResult?.tickets) {
                    return;
                }

                for (const ticket of queryResult.tickets) {
                    ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description));
                }

                sorting = queryResult.sorting!;
                filter = queryResult.filter!;
                tickets = queryResult.tickets;
            } catch (ex) {
                console.log(ex)
                toast.error("Failed to fetch tickets.");
            }
        }, 500);
    }
</script>

<section class="tickets">
    <h2>Tickets</h2>
    <div class="search-area">
        <Input placeholder="Search..."
               bind:value={searchQuery}
               on:input={query} />
        <div class="sorting">
            <Select bind:choices={filterKeys}
                    bind:selectedValue={filterString}
                    on:change={query} />
            <Select choices={Object.keys(sortingMap)}
                    bind:selectedValue={sortingString}
                    on:change={query} />
        </div>
    </div>
    {#each tickets as ticket}
        <TicketEntry bind:ticket={ticket} bind:isOverview={isOverview} />
    {/each}
</section>

<style lang="sass">
    section
        border-bottom: var(--border)
        margin-bottom: 1em
        padding-bottom: 1em

    .search-area
        display: flex
        gap: 0.8em
        margin-bottom: 0.8em

        .sorting
            display: flex
            gap: var(--spacing)
            width: 100%
            max-width: 37em

    @media screen and (max-width: 790px)
        .search-area
            flex-direction: column
</style>
