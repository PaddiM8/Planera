<script lang="ts">
    import type {ProjectDto, TicketDto} from "../../../../../gen/planeraClient";
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

    export let data: {
        project: ProjectDto,
        sorting: TicketSorting,
        filter: TicketFilter,
        tickets: TicketDto[],
    };
    export let form: {
        errors: { string: string[] } | undefined,
    };

    let editor;
    let titleValue: string;
    let titleInput: HTMLInputElement;
    let assignees;
    let priority;
    let searchQuery: string;
    const filterMap = {
        "All": TicketFilter.All,
        "Open": TicketFilter.Open,
        "Closed": TicketFilter.Closed,
        "Inactive": TicketFilter.Inactive,
        "Done": TicketFilter.Done,
        "Assigned to Me": TicketFilter.AssignedToMe,
    };
    const sortingMap = {
        "Newest": TicketSorting.Newest,
        "Oldest": TicketSorting.Oldest,
        "Highest Priority": TicketSorting.HighestPriority,
        "Lowest Priority": TicketSorting.LowestPriority,
    };
    let sorting: string;
    let filter: string;
    let queryTimeout: number;
    let isFormLoading = false;
    let reachedEndOfTickets = false;
    let isLoadingMore = false;

    $: isSubmitDisabled = titleValue?.length < 2 || isFormLoading;
    $: validFormState = titleValue?.length >= 2;
    $: updateSortingWithoutQuerying(data?.sorting, data?.filter);

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
        document.getElementById("main-area").onscroll = e => {
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
            data.project.author.username,
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
                data.tickets[index][key] = value;
            }
        }
    }

    async function beforeSubmit({ formData }) {
        isFormLoading = true;
        formData.append("description", await editor.getHtml());
    }

    function afterSubmit(success: boolean) {
        if (success) {
            titleValue = "";
            editor.reset();
            priority.reset();
            assignees.reset();
            setTimeout(() => {
                titleInput.focus();
            }, 100);
            searchQuery = "";
            toast.info("Created ticket successfully.");
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
                    data.project.author.username,
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
            <UserIcon name={data.project.name}
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
        <Editor placeholder="Describe the ticket..."
                bind:this={editor} />
        <div class="bottom-row">
            <span class="group">
                <span class="label">
                    <Label value="Priority" />
                </span>
                <MultiButton name="priority"
                             choices={["None", "Low", "Normal", "High", "Severe"]}
                             defaultChoice="Normal"
                             bind:this={priority} />
            </span>
            <span class="group">
                <span class="label">
                    <Label value="Assigned To" />
                </span>
                <BlockInput placeholder="Assignee..."
                            options={$participants}
                            key="username"
                            outputKey="id"
                            name="assignee"
                            bind:this={assignees}
                            showUserIcons={true} />
            </span>
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
            <Select choices={Object.keys(filterMap)}
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

    .tickets
        h2
            margin-bottom: 0.6em

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