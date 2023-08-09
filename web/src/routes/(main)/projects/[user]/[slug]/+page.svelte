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
    import {projectHub} from "./store";
    import {getAvatarUrl} from "$lib/clients";
    import UserIcon from "$lib/components/UserIcon.svelte";

    export let data: {
        project: ProjectDto,
        tickets: TicketDto[],
    };
    export let form: {
        errors: { string: string[] } | undefined,
    };


    onMount(async () => {
        projectHub.subscribe(hub => {
            if (!hub) {
                return;
            }

            hub.on("onTicketUpdate", onTicketUpdate);
        });
    });

    function onTicketUpdate(projectId: number, ticketId: number, newFields: TicketDto) {
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