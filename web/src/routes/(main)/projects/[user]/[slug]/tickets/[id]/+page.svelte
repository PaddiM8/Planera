<script lang="ts">
    import type {TicketDto} from "../../../../../../../gen/planeraClient";
    import {getAvatarUrl} from "$lib/clients";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {participants} from "../../../../../store";
    import Label from "$lib/components/GroupLabel.svelte";
    import BlockInput from "$lib/components/form/BlockInput.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import PriorityLabel from "$lib/components/ticket/PriorityLabel.svelte";
    import {PencilSquare, Trash} from "svelte-hero-icons";
    import Editor from "$lib/components/editor/Editor.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {dialog} from "$lib/dialog";
    import {toast} from "$lib/toast";
    import {TicketPriority, UserDto} from "../../../../../../../gen/planeraClient";
    import {projectHub} from "../../store";
    import {beforeNavigate, goto} from "$app/navigation";
    import NoteEntry from "$lib/components/ticket/NoteEntry.svelte";
    import IconButton from "$lib/components/IconButton.svelte";
    import {onMount} from "svelte";
    import BackButton from "$lib/components/BackButton.svelte";
    import type {ProblemDetails} from "$lib/problemDetails";

    export let form: {
        problem: ProblemDetails,
        addNoteProblem: ProblemDetails,
        editNoteProblem: ProblemDetails,
    };
    export let data: {
        ticket: TicketDto,
    };

    let isEditing = false;
    let editor;
    let selectedPriorityName: string;
    let previousPriority = data?.ticket?.priority;

    onMount(() => {
        selectedPriorityName = TicketPriority[data?.ticket?.priority ?? 0];
    });

    async function beforeSubmit({ formData }) {
        formData.append("description", await editor.getHtml());
    }

    function afterSubmit(success: boolean) {
        if (success) {
            isEditing = false;
            toast.info("Updated ticket successfully.");
        }
    }

    async function handleCancel() {
        if (await dialog.yesNo("Unsaved Changes", "Are you sure you want to cancel?")) {
            isEditing = false;
        }
    }

    function handleEdit() {
        isEditing = true;
        editor.setHtml(data.ticket.description);
    }

    async function handleRemove() {
        if (!await dialog.yesNo("Remove ticket", "Are you sure you want to remove the ticket?")) {
            return;
        }

        try {
            await $projectHub!.invoke(
                "removeTicket",
                data.ticket.projectId,
                data.ticket.id,
            );

            toast.info("Removed ticket successfully");
            await goto("../")
        } catch {
            toast.error("Failed to remove ticket.");
        }
    }

    async function handlePriorityChange(e: CustomEvent) {
        const priority = TicketPriority[e.detail];
        try {
            await $projectHub!.invoke(
                "setTicketPriority",
                data.ticket.projectId,
                data.ticket.id,
                priority,
            );

            toast.info("Changed priority successfully.", 1500);
            data.ticket.priority = priority;
            previousPriority = priority;
        } catch {
            toast.error("Failed to change priority.");
            if (previousPriority) {
                data.ticket.priority = previousPriority;
            }
        }
    }

    async function handleAddAssignee(e: CustomEvent) {
        const assignee = e.detail as UserDto;
        try {
            await $projectHub!.invoke(
                "addTicketAssignee",
                data.ticket.projectId,
                data.ticket.id,
                assignee.id
            );

            toast.info("Added assignee successfully.");
        } catch {
            toast.error("Failed to add assignee.");
            data.ticket.assignees = data.ticket.assignees.filter(x => x.id !== assignee.id);
        }
    }

    async function handleRemoveAssignee(e: CustomEvent) {
        const assignee = e.detail as UserDto;
        try {
            await $projectHub!.invoke(
                "removeTicketAssignee",
                data.ticket.projectId,
                data.ticket.id,
                assignee.id
            );

            toast.info("Removed assignee successfully.");
        } catch {
            toast.error("Failed to remove assignee.");
            data.ticket.assignees = [...data.ticket.assignees, assignee];
        }
    }
</script>
<BackButton placeName={data?.ticket.project.name} />
<div class="edit-area" class:hidden={!isEditing}>
    <Form action="?/edit" {beforeSubmit} {afterSubmit} problem={form?.problem} reset={false}>
        <input type="hidden" name="projectId" value={data.ticket.projectId} />
        <input type="hidden" name="ticketId" value={data.ticket.id} />

        <Input type="text" name="title" placeholder="Title..." value={data.ticket.title} />
        <Editor placeholder="Describe the ticket..." bind:this={editor} />
        <div class="buttons">
            <Button value="Cancel" on:click={handleCancel} />
            <Button value="Edit" primary submit />
        </div>
    </Form>
</div>
<div class="ticket" class:hidden={isEditing}>
    <div class="top">
        <h2>{data.ticket.title}</h2>
        <div class="icons">
            <IconButton icon={Trash} on:click={handleRemove} />
            <IconButton icon={PencilSquare} on:click={handleEdit} />
        </div>
    </div>
    <div class="about">
        <div class="author">
            <div class="avatar">
                <UserIcon name={data.ticket.author.username}
                          image={getAvatarUrl(data.ticket.author.avatarPath, "small")}
                          type="user" />
            </div>
            <span class="name">{data.ticket.author.username}</span>
        </div>
    </div>
    <div class="description">{@html data.ticket.description}</div>
    <PriorityLabel bind:priority={data.ticket.priority} />
</div>

<div class="bottom-row">
    <span class="group">
        <span class="label">
            <Label value="Priority" />
        </span>
        <MultiButton name="priority"
                     choices={["None", "Low", "Normal", "High", "Severe"]}
                     defaultChoice="Normal"
                     bind:selectedValue={selectedPriorityName}
                     on:change={handlePriorityChange}/>
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
                    showUserIcons={true}
                    bind:values={data.ticket.assignees}
                    on:add={handleAddAssignee}
                    on:remove={handleRemoveAssignee} />
    </span>
</div>

<hr>
<h2>Notes</h2>
<Form action="?/addNote"
      problem={form?.addNoteProblem}
      horizontal>
    <input type="hidden" name="projectId" value={data.ticket.projectId} />
    <input type="hidden" name="ticketId" value={data.ticket.id} />

    <Input name="content" placeholder="Write something..." />
    <Button primary submit value="Add" />
</Form>

<section class="notes">
    {#each data?.ticket.notes as note}
        <NoteEntry bind:note
                   editAction="?/editNote"
                   problem={form?.editNoteProblem}
                   on:remove={e => data.ticket.notes = data.ticket.notes.filter(x => x.id !== e.detail)} />
    {/each}
</section>

<style lang="sass">
    :global(.hidden)
        display: none

    h2
        margin-top: 0.2em
        margin-bottom: 0.6em

    .top
        display: flex
        align-items: center

        .icons
            display: flex
            align-self: flex-start
            margin-top: 0.4em
            gap: 0.5em
            margin-left: auto
            color: var(--on-background-inactive)

        :global(.icons > *)
            font-size: 1em

    :global(.top .icons > *)
        cursor: pointer

        &:hover
            color: var(--on-background)

    .about
        margin-bottom: 1em

    .author
        display: flex
        align-items: center
        gap: 0.3em

        .avatar
            width: 1.2em
            font-size: 1.1em

        .name
            font-size: 1em
            font-weight: 500
            color: var(--text-gray)

    .buttons
        display: flex
        gap: 0.4em

    .ticket
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)
        margin-bottom: 0.4em

        h2
            font-size: 1.4em

    .description
        padding-bottom: 0.4em

    :global(.ticket .description p)
        margin: 0

    .group
        display: flex
        flex-direction: column
        gap: 0.05em

        &:last-of-type
            flex-grow: 1

    .label
        font-size: 0.9em

    .bottom-row
        display: flex
        align-items: center
        gap: 0.8em

    .notes
        display: flex
        flex-direction: column
        gap: var(--spacing)
        margin-top: var(--spacing)

    @media screen and (max-width: 980px)
        .bottom-row
            flex-direction: column
            align-items: normal
</style>