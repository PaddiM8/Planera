<script lang="ts">
    import type {TicketDto} from "../../../../../../../gen/planeraClient";
    import {getAvatarUrl} from "$lib/clients";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {participants} from "../../../../../store";
    import Label from "$lib/components/GroupLabel.svelte";
    import BlockInput from "$lib/components/form/BlockInput.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import PriorityLabel from "$lib/components/ticket/PriorityLabel.svelte";
    import {Check, Icon, Minus, PencilSquare, Trash, XMark} from "svelte-hero-icons";
    import Editor from "$lib/components/editor/Editor.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {dialog} from "$lib/dialog";
    import {toast} from "$lib/toast";
    import {TicketPriority, TicketStatus, UserDto} from "../../../../../../../gen/planeraClient";
    import {projectHub} from "../../store";
    import {goto} from "$app/navigation";
    import NoteEntry from "$lib/components/ticket/NoteEntry.svelte";
    import IconButton from "$lib/components/IconButton.svelte";
    import {onMount} from "svelte";
    import BackButton from "$lib/components/BackButton.svelte";
    import type {ProblemDetails} from "$lib/problemDetails";
    import type {FormSubmitInput} from "../../../../../../types";
    import {truncate} from "$lib/util";

    export let form: {
        problem: ProblemDetails,
        addNoteProblem: ProblemDetails,
        editNoteProblem: ProblemDetails,
    };
    export let data: {
        ticket: TicketDto,
    };

    let isEditing = false;
    let editor: any;
    let selectedPriorityName: string;
    let previousPriority = data?.ticket?.priority;

    onMount(() => {
        selectedPriorityName = TicketPriority[data?.ticket?.priority ?? 0];
        projectHub.subscribe(hub => hub?.on("onUpdateTicket", onUpdateTicket));
    });

    function onUpdateTicket(projectId: string, ticketId: number, newFields: TicketDto) {
        for (const [key, value] of Object.entries(newFields)) {
            (data.ticket as { [key: string]: any })[key] = value;
        }
    }

    async function beforeSubmit({ formData }: FormSubmitInput) {
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

    async function setStatus(status: TicketStatus) {
        await $projectHub?.invoke(
            "setTicketStatus",
            data.ticket.projectId,
            data.ticket.id,
            status
        );
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
            data.ticket.priority = priority as unknown as TicketPriority;
            previousPriority = priority as unknown as TicketPriority;
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

<svelte:head>
    <title>{truncate(data.ticket.title, 25)} - Planera</title>
</svelte:head>

<div class="header">
    <BackButton placeName={data?.ticket.project?.name} />

    {#if data.ticket.status === TicketStatus.None}
        <div class="status-buttons">
            <IconButton value="Close"
                        icon={XMark}
                        color="red"
                        on:click={() => setStatus(TicketStatus.Closed)} />
            <IconButton value="Inactive"
                        icon={Minus}
                        color="blue"
                        on:click={() => setStatus(TicketStatus.Inactive)} />
            <IconButton value="Done"
                        icon={Check}
                        color="green"
                        on:click={() => setStatus(TicketStatus.Done)} />
        </div>
    {/if}
</div>

<div class="edit-area" class:hidden={!isEditing}>
    <Form action="?/edit"
          {beforeSubmit}
          {afterSubmit}
          promptWhenModified
          problem={form?.problem}
          reset={false}>
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
        {#if data.ticket.status === TicketStatus.Done}
            <button class="status done" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Check} />
            </button>
        {:else if data.ticket.status === TicketStatus.Inactive}
            <button class="status inactive" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Minus} />
            </button>
        {:else if data.ticket.status === TicketStatus.Closed}
            <button class="status closed" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={XMark} />
            </button>
        {/if}
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
    <PriorityLabel bind:priority={data.ticket.priority}
                   bind:status={data.ticket.status} />
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
      promptWhenModified
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

    .status
        display: block
        margin-top: 0.1em
        margin-right: 0.1em
        height: 1.5em
        min-width: 1.5em
        cursor: pointer

        &.done
            color: var(--green)

        &.inactive
            color: var(--blue)

        &.closed
            color: var(--red)

    :global(.status svg)
        stroke-width: 2

    .header
        display: flex
        gap: 0.4rem
        align-items: center
        justify-content: space-between
        flex-wrap: wrap
        margin-bottom: 0.4rem

        .status-buttons
            flex: 1
            display: flex
            gap: 0.4em
            justify-content: end
            flex-wrap: nowrap
    .top
        display: flex
        align-items: flex-start
        padding-top: 0.2em
        overflow: hidden

        h2
            margin: -0.1em auto 0.4em 0.1em
            overflow: hidden

        .icons
            display: flex
            gap: 0.5em
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

    :global(.ticket .description img)
        max-width: 40em

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
