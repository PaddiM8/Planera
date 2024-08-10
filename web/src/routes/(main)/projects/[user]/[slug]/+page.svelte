<script lang="ts">
    import {type ProjectDto, type TicketDto, TicketStatus} from "../../../../../gen/planeraClient";
    import {TicketFilter, TicketSorting} from "../../../../../gen/planeraClient";
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Editor from "$lib/components/editor/Editor.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import Label from "$lib/components/GroupLabel.svelte";
    import BlockInput from "$lib/components/form/BlockInput.svelte";
    import TicketEntry from "$lib/components/ticket/TicketEntry.svelte";
    import {onMount} from "svelte";
    import {participants} from "../../../store";
    import {projectHub, ticketsPerPage} from "./store";
    import {getAvatarUrl} from "$lib/clients";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {toast} from "$lib/toast";
    import Select from "$lib/components/form/Select.svelte";
    import {makeImagePathsAbsolute} from "$lib/paths";
    import {sanitizeHtml} from "$lib/formatting";
    import {getKeyFromValue} from "$lib/util.js";
    import type {FormSubmitInput} from "../../../../types";
    import type {ProblemDetails} from "$lib/problemDetails";

    export let data: {
        project: ProjectDto,
        sorting: TicketSorting,
        filter: TicketFilter,
        tickets: TicketDto[],
    };
    export let form: {
        errors: { string: string[] } | undefined,
        problem: ProblemDetails,
    };

    let editor: any | undefined;
    let titleValue: string;
    let titleInput: Input;
    let assigneesInput: BlockInput;
    let priorityInput: MultiButton;
    let searchQuery: string;
    const filterMap: { [key: string]: TicketFilter } = {};
    let filterKeys: string[] = [];

    const sortingMap: { [key: string]: TicketSorting } = {
        "Newest": TicketSorting.Newest,
        "Oldest": TicketSorting.Oldest,
        "Highest Priority": TicketSorting.HighestPriority,
        "Lowest Priority": TicketSorting.LowestPriority,
    };
    let sorting: string;
    let filter: string;
    let queryTimeout: NodeJS.Timeout;
    let isFormLoading = false;
    let reachedEndOfTickets = false;
    let isLoadingMore = false;
    refreshFilterMap();

    $: isSubmitDisabled = titleValue?.length < 2 || isFormLoading;
    $: validFormState = titleValue?.length >= 2;
    $: updateSortingWithoutQuerying(data?.sorting, data?.filter);

    function refreshFilterMap() {
        for (const key in filterMap) {
            delete filterMap[key];
        }

        filterMap[`All (${data.project.allTicketsCount})`] = TicketFilter.All;
        filterMap[`Open (${data.project.openTicketsCount})`] = TicketFilter.Open;
        filterMap[`Closed (${data.project.closedTicketsCount})`] = TicketFilter.Closed;
        filterMap[`Inactive (${data.project.inactiveTicketsCount})`] = TicketFilter.Inactive;
        filterMap[`Done (${data.project.doneTicketsCount})`] = TicketFilter.Done;
        filterMap[`Assigned to Me (${data.project.assignedToMeCount})`] = TicketFilter.AssignedToMe;
        filterKeys = Object.keys(filterMap);

        // Since the keys changed, the currently selected filter value needs to change
        // as well
        if (filter) {
            setFilter(filter.split(" ").at(0) ?? "");
        }
    }

    function setFilter(name: string) {
        filter = filterKeys.find(key => key.startsWith(name))
            ?? filterKeys[0];
    }

    function updateSortingWithoutQuerying(newSorting?: TicketSorting, newFilter?: TicketFilter) {
        const newSortingString = getKeyFromValue(sortingMap, newSorting ?? TicketSorting.Newest)!;
        const newFilterString = getKeyFromValue(filterMap, newFilter ?? TicketFilter.All)!;
        if (sorting  == undefined || newSortingString !== sorting || filter == undefined || newFilterString !== filter) {
            sorting = newSortingString;
            filter = newFilterString;
        }
    }

    onMount(async () => {
        localStorage.setItem("lastVisited", window.location.pathname);
        const mainArea = document.getElementById("main-area") as HTMLElement;
        mainArea.onscroll = e => {
            const target = e.target as HTMLElement;
            if (!isLoadingMore && target.scrollTop + target.clientHeight >= target.scrollHeight - 100) {
                loadMore();
            }
        }

        projectHub.subscribe(hub => hub?.on("onUpdateTicket", onUpdateTicket));
    });

    async function loadMore() {
        if (reachedEndOfTickets || !$projectHub) {
            return;
        }

        isLoadingMore = true;
        const queryResult = await $projectHub!.invoke(
            "queryTickets",
            data.project.author?.username,
            data.project.slug,
            data.tickets.length,
            ticketsPerPage,
            searchQuery,
            sortingMap[sorting],
            filterMap[filter],
        );

        for (const ticket of queryResult.tickets) {
            ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description));
        }

        data.tickets = [...data.tickets, ...queryResult.tickets];

        if (queryResult.tickets.length === 0) {
            reachedEndOfTickets = true;
        }

        isLoadingMore = false;
    }

    function onUpdateTicket(projectId: string, ticketId: number, newFields: TicketDto) {
        const index = data.tickets.findIndex(x => x.id === ticketId);
        if (index !== -1) {
            for (const [key, value] of Object.entries(newFields)) {
                const ticket = (data.tickets as any)[index];
                if (key === "status") {
                    updateTicketCounts(value, ticket.status);
                }

                ticket[key] = value;
            }
        }
    }

    function updateTicketCounts(newStatus: TicketStatus, oldStatus: TicketStatus) {
        if (data.project.openTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.None) {
                data.project.openTicketsCount--;
            } else if (newStatus === TicketStatus.None) {
                data.project.openTicketsCount++;
            }
        }

        if (data.project.closedTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Closed) {
                data.project.closedTicketsCount--;
            } else if (newStatus === TicketStatus.Closed) {
                data.project.closedTicketsCount++;
            }
        }

        if (data.project.inactiveTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Inactive) {
                data.project.inactiveTicketsCount--;
            } else if (newStatus === TicketStatus.Inactive) {
                data.project.inactiveTicketsCount++;
            }
        }

        if (data.project.doneTicketsCount !== undefined) {
            if (oldStatus === TicketStatus.Done) {
                data.project.doneTicketsCount--;
            } else if (newStatus === TicketStatus.Done) {
                data.project.doneTicketsCount++;
            }
        }

        refreshFilterMap();
    }

    async function beforeSubmit({ formData }: FormSubmitInput) {
        isFormLoading = true;
        const description = editor ? await editor.getHtml() : "";
        formData.append("description", description);
    }

    function afterSubmit(success: boolean) {
        if (success) {
            titleValue = "";
            editor?.reset();
            priorityInput?.reset();
            assigneesInput?.reset();
            setTimeout(() => {
                titleInput?.focus();
            }, 100);
            searchQuery = "";
            toast.info("Created ticket successfully.");

            if (data.project.allTicketsCount !== undefined && data.project.openTicketsCount !== undefined) {
                refreshFilterMap();
            }
        }

        // Wait a little bit before enabling the button again
        // to prevent ugly flickering.
        setTimeout(() => {
            isFormLoading = false;
        }, 500);
    }

    function query() {
        reachedEndOfTickets = false;
        const newTimeout = setTimeout(async () => {
            clearTimeout(queryTimeout);
            queryTimeout = newTimeout;

            try {
                const queryResult = await $projectHub!.invoke(
                    "queryTickets",
                    data.project.author?.username,
                    data.project.slug,
                    0,
                    ticketsPerPage,
                    searchQuery,
                    sortingMap[sorting],
                    filterMap[filter],
                );

                for (const ticket of queryResult.tickets) {
                    ticket.description = sanitizeHtml(makeImagePathsAbsolute(ticket.description));
                }

                data.sorting = queryResult.sorting;
                data.filter = queryResult.filter;
                data.tickets = queryResult.tickets;
            } catch {
                toast.error("Failed to fetch tickets.");
            }
        }, 500);
    }
</script>

<svelte:head>
    <title>{data.project.name} - Planera</title>
</svelte:head>

<section class="description">
    <div class="top">
        <div class="icon">
            <UserIcon name={data.project.name ?? ""}
                      image={getAvatarUrl(data.project.iconPath, "big")}
                      type="project" />
        </div>
        <h1>{data.project.name}</h1>
    </div>
    <h3>{data.project.description}</h3>
</section>

<section class="new-ticket">
    <h2>New Ticket</h2>
    <Form {beforeSubmit}
          {afterSubmit}
          promptWhenModified
          problem={form?.problem}
          bind:validState={validFormState}>
        <input type="hidden" name="projectId" value={data.project.id} />

        <Input type="text"
               name="title"
               placeholder="Title..."
               bind:value={titleValue}
               bind:this={titleInput} />

        {#if data.project.enableTicketDescriptions}
            <Editor placeholder="Describe the ticket..."
                    bind:this={editor} />
        {/if}

        <div class="bottom-row">
            <span class="group">
                <span class="label">
                    <Label value="Priority" />
                </span>
                <MultiButton name="priority"
                             choices={["None", "Low", "Normal", "High", "Severe"]}
                             defaultValue="Normal"
                             bind:this={priorityInput} />
            </span>

            {#if data.project.enableTicketAssignees}
                <span class="group">
                    <span class="label">
                        <Label value="Assigned To" />
                    </span>
                    <BlockInput placeholder="Assignee..."
                                options={$participants}
                                key="username"
                                outputKey="id"
                                name="assignee"
                                bind:this={assigneesInput}
                                showUserIcons={true} />
                </span>
            {/if}

            <Button value="Create"
                    primary
                    submit
                    bind:disabled={isSubmitDisabled} />
        </div>
    </Form>
</section>

<section class="tickets">
    <h2>Tickets</h2>
    <div class="search-area">
            <Input placeholder="Search..."
                   bind:value={searchQuery}
                   on:input={query} />
        <div class="sorting">
            <Select bind:choices={filterKeys}
                    bind:selectedValue={filter}
                    on:change={query} />
            <Select choices={Object.keys(sortingMap)}
                    bind:selectedValue={sorting}
                    on:change={query} />
        </div>
    </div>
    {#each data.tickets as ticket}
        <TicketEntry bind:ticket={ticket} />
    {/each}
</section>

<style lang="sass">
    section
        border-bottom: var(--border)
        margin-bottom: 1em
        padding-bottom: 1em

    .description
        .top
            display: flex
            align-items: center
            gap: 0.5em
            margin-bottom: 0.4em

            h1
                margin-bottom: 0

            .icon
                width: 1.2em
                font-size: 2em

    .group
        display: flex
        flex-direction: column
        gap: 0.05em
        margin-top: -0.4em

        &:last-of-type
            flex-grow: 1

    .label
        font-size: 0.9em

    .bottom-row
        display: flex
        align-items: center
        gap: 0.8em

    .search-area
        display: flex
        gap: 0.8em
        margin-bottom: 0.8em

        .sorting
            display: flex
            gap: var(--spacing)
            width: 100%
            max-width: 37em

    @media screen and (max-width: 980px)
        .bottom-row
            align-items: normal
            flex-direction: column

    @media screen and (max-width: 790px)
        .search-area
            flex-direction: column
</style>