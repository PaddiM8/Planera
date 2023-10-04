<script lang="ts">
    import {Check, Icon, Minus, PencilSquare, Trash, XMark} from "svelte-hero-icons";
    import {NoteDto, TicketStatus} from "../../../gen/planeraClient";
    import IconButton from "$lib/components/IconButton.svelte";
    import {dialog} from "$lib/dialog";
    import {projectHub} from "../../../routes/(main)/projects/[user]/[slug]/store";
    import {toast} from "$lib/toast";
    import {createEventDispatcher} from "svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";
    import {formatDate} from "$lib/formatting";
    import type {ProblemDetails} from "$lib/problemDetails";

    export let note: NoteDto;
    export let editAction: string;
    export let problem: ProblemDetails;

    let isEditing = false;
    let editedContent: string;
    const dispatcher = createEventDispatcher();

    async function setStatus(status: TicketStatus) {
        try {
            await $projectHub!.invoke("setNoteStatus", note.id, status);
            toast.info("Note status changed successfully.");
            note.status = status;
        } catch {
            toast.error("Failed to change note status.");
        }
    }

    async function handleRemove() {
        if (!await dialog.yesNo("Remove note", "Are you sure you want to remove this note?")) {
            return;
        }

        try {
            await $projectHub!.invoke("removeNote", note.id);
            toast.info("Removed note successfully.");
            dispatcher("remove", note.id);
        } catch {
            toast.error("Failed to remove note.");
        }
    }

    async function handleEdit() {
        editedContent = note.content;
        isEditing = true;
    }

    function afterSubmit(success: boolean) {
        if (success) {
            isEditing = false;
        }
    }
</script>

<div class="note" class:has-status={note.status !== TicketStatus.None}>
    <div class="top">
        {#if note.status === TicketStatus.Done}
            <button class="status done" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Check} />
            </button>
        {:else if note.status === TicketStatus.Inactive}
            <button class="status inactive" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={Minus} />
            </button>
        {:else if note.status === TicketStatus.Closed}
            <button class="status closed" on:click={() => setStatus(TicketStatus.None)}>
                <Icon src={XMark} />
            </button>
        {/if}
        <h3 class="user">{note.author?.username}</h3>
        {#if !isEditing}
            <div class="buttons status-buttons">
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
            <div class="buttons">
                <IconButton icon={Trash}
                            on:click={handleRemove} />
                <IconButton icon={PencilSquare}
                             on:click={handleEdit} />
            </div>
        {/if}
    </div>
    {#if isEditing}
        <Form action={editAction}
              {afterSubmit}
              {problem}
              reset={false}
              horizontal
              smallMargins>
            <input type="hidden" name="id" value={note.id} />

            <Input name="content" value={editedContent} placeholder="Write something..." />
            <Button value="Cancel" on:click={() => isEditing = false} />
            <Button value="Edit" primary submit />
        </Form>
    {:else}
        <div class="bottom">
            <p class="content">{note.content}</p>
            <time>{formatDate(note.timestamp)}</time>
        </div>
    {/if}
</div>

<style lang="sass">
    .note
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

        &:not(:hover) .buttons, &.has-status .status-buttons
            visibility: hidden

        .top
            display: flex
            align-items: center

        .user
            font-size: 0.9em
            color: var(--text-gray)

        .content
            margin: 0.2em 0

    .status
        display: block
        height: 1.2em
        margin-right: 0.2em
        cursor: pointer

        &.done
            color: var(--green)

        &.inactive
            color: var(--blue)

        &.closed
            color: var(--red)

    .status-buttons
        margin-left: auto
        margin-right: 0.4em

    .buttons
        display: flex
        align-items: center
        gap: 0.4em

    .bottom
        display: flex
        align-items: flex-end

        .content
            flex-grow: 1

        time
            color: var(--text-gray)
            font-size: 0.8em
            font-weight: 500
</style>