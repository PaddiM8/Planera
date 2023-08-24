<script lang="ts">
    import type {ProjectDto, TicketDto} from "../../../../../gen/planeraClient";
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
    import {TicketSorting, TicketStatus} from "../../../../../gen/planeraClient";
    import {beforeNavigate} from "$app/navigation";

    export let data: {
        project: ProjectDto,
        tickets: TicketDto[],
    };
    export let form: {
        errors: { string: string[] } | undefined,
    };

    let modified = false;
    let reachedEndOfTickets = false;

    beforeNavigate(({ cancel }) => {
        if (modified && !confirm("Are you sure you want to leave this page? You have unsaved changes that will be lost.")) {
            cancel();
        }
    });

    onMount(async () => {
        window.onbeforeunload = () => {
            if (modified) {
                return true;
            }
        }

        document.getElementById("main-area").onscroll = e => {
            const target = e.target as HTMLElement;
            if (target.scrollTop + target.clientHeight >= target.scrollHeight - 100) {
                loadMore();
            }
        }

        projectHub.subscribe(hub => {
            if (!hub) {
                return;
            }

            hub.on("onUpdateTicket", onUpdateTicket);
        });
    });

    async function loadMore() {
        if (reachedEndOfTickets) {
            return;
        }

        const newTickets = await $projectHub!.invoke(
            "queryTickets",
            data.project.author.username,
            data.project.slug,
            data.tickets.length,
            ticketsPerPage,
            searchQuery,
            sortingMap[sorting],
            filterMap[filterByStatus],
        );
        data.tickets = [...data.tickets, ...newTickets];
        if (newTickets.length === 0) {
            reachedEndOfTickets = true;
        }
    }

    function onUpdateTicket(projectId: string, ticketId: number, newFields: TicketDto) {
        const index = data.tickets.findIndex(x => x.id === ticketId);
        if (index !== -1) {
            for (const [key, value] of Object.entries(newFields)) {
                data.tickets[index][key] = value;
            }
        }
    }

    let editor;
    let assignees;
    let priority;
    let searchQuery: string;
    let filterByStatus: string = "All";
    let sorting: string = "Newest";
    const filterMap = {
        "All": null,
        "Open": TicketStatus.None,
        "Closed": TicketStatus.Closed,
        "Inactive": TicketStatus.Inactive,
        "Done": TicketStatus.Done,
    };
    const sortingMap = {
        "Newest": TicketSorting.Newest,
        "Oldest": TicketSorting.Oldest,
        "Highest Priority": TicketSorting.HighestPriority,
        "Lowest Priority": TicketSorting.LowestPriority,
    };
    let queryTimeout: number;

    async function beforeSubmit({ formData }) {
        formData.append("description", await editor.getHtml());
    }

    function afterSubmit(success: boolean) {
        if (success) {
            modified = false;
            editor.reset();
            priority.reset();
            assignees.reset();
            toast.info("Created ticket successfully.");
        }
    }

    function query() {
        reachedEndOfTickets = false;
        const newTimeout = setTimeout(async () => {
            clearTimeout(queryTimeout);
            queryTimeout = newTimeout;

            try {
                data.tickets = await $projectHub!.invoke(
                    "queryTickets",
                    data.project.author.username,
                    data.project.slug,
                    0,
                    ticketsPerPage,
                    searchQuery,
                    sortingMap[sorting],
                    filterMap[filterByStatus],
                );
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
    <Form {beforeSubmit} {afterSubmit} problem={form?.problem}>
        <input type="hidden" name="projectId" value={data.project.id} />

        <Input type="text" name="title" placeholder="Title..." on:input={() => modified = true} />
        <Editor placeholder="Describe the ticket..." bind:this={editor} on:input={() => modified = true} />
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
            <Button value="Create" primary submit />
        </div>
    </Form>
</section>

<section class="tickets">
    <h2>Tickets</h2>
    <div class="search-area" style="display:flex; gap: 0.8em; margin-bottom: 0.8em">
        <Input placeholder="Search..."
               bind:value={searchQuery}
               on:input={query} />
        <Select choices={Object.keys(filterMap)}
                bind:selectedValue={filterByStatus}
                on:change={query} />
        <Select choices={Object.keys(sortingMap)}
                bind:selectedValue={sorting}
                on:change={query} />
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
</style>