<script lang="ts">
    import type {ProjectDto, TicketDto} from "../../../../../gen/planeraClient";
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Editor from "$lib/components/editor/Editor.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import Label from "$lib/components/form/Label.svelte";
    import BlockInput from "$lib/components/form/BlockInput.svelte";
    import TicketEntry from "$lib/components/TicketEntry.svelte";
    import {getProjectHub, startProjectHub} from "$lib/hubs";
    import {toast} from "$lib/toast";
    import {onMount} from "svelte";
    import {browser} from "$app/environment";

    export let data: {
        project: ProjectDto,
        tickets: TicketDto[],
    };
    export let form: {
        errors: { string: string[] } | undefined,
    };

    let previousProjectId: number | undefined = undefined;

    async function connectToProject(projectId) {
        if (!browser || !getProjectHub()) {
            return;
        }

        if (previousProjectId == projectId)
            return;

        if (previousProjectId) {
            try {
                await getProjectHub().invoke("leave", previousProjectId);
            } catch {}
        }

        try {
            await getProjectHub().invoke("join", projectId);
        } catch (ex) {
            toast.error("Failed to connect to project.");
        }

        previousProjectId = projectId;
    }

    $: connectToProject(data?.project.id);

    onMount(async () => {
        await startProjectHub();
        await connectToProject(data.project.id);
        getProjectHub().on("getTicketUpdate", onTicketUpdate);
    });

    function onTicketUpdate(projectId: number, ticketId: number, ticketStatus: TicketDto) {
        const index = data.tickets.findIndex(x => x.id === ticketId);
        if (index !== -1) {
            for (const [key, value] of Object.entries(ticketStatus)) {
                data.tickets[index][key] = value;
            }
        }
    }

    let editor;
    let assignees;
    let priority;

    async function beforeSubmit({ formData }) {
        formData.append("description", await editor.getHtml());
    }

    function afterSubmit(success: boolean) {
        if (success) {
            editor.reset();
            priority.reset();
            assignees.reset();
        }
    }
</script>

<svelte:head>
    <title>{data.project.name} - Planera</title>
</svelte:head>

<section class="description">
    <h1>{data.project.name}</h1>
    <h3>{data.project.description}</h3>
</section>

<section class="new-ticket">
    <h2>New Ticket</h2>
    <Form action="?/create" {beforeSubmit} {afterSubmit} errors={form?.errors}>
        <input type="hidden" name="projectId" value={data.project.id} />

        <Input type="text" name="title" placeholder="Title..." />
        <Editor placeholder="Describe the ticket..." bind:this={editor} />
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
                            options={data.project.participants}
                            key="userName"
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
    {#each data?.tickets as ticket}
        <TicketEntry bind:ticket={ticket} />
    {/each}
</section>

<style lang="sass">
    section
        border-bottom: var(--border)
        margin-bottom: 1em
        padding-bottom: 1em

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