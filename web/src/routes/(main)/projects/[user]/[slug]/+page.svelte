<script lang="ts">
    import {type ProjectDto, type TicketDto} from "../../../../../gen/planeraClient";
    import {TicketFilter, TicketSorting} from "../../../../../gen/planeraClient";
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Editor from "$lib/components/editor/Editor.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import Label from "$lib/components/GroupLabel.svelte";
    import BlockInput from "$lib/components/form/BlockInput.svelte";
    import {onMount} from "svelte";
    import {participants} from "../../../store";
    import {getAvatarUrl} from "$lib/clients";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {toast} from "$lib/toast";
    import type {FormSubmitInput} from "../../../../types";
    import type {ProblemDetails} from "$lib/problemDetails";
    import TicketList from "$lib/components/ticket/TicketList.svelte";

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
    let isFormLoading = false;
    let ticketListElement: TicketList | undefined;

    $: isSubmitDisabled = titleValue?.length < 2 || isFormLoading;
    $: validFormState = titleValue?.length >= 2;

    onMount(async () => {
        localStorage.setItem("lastVisited", window.location.pathname);
    });

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
            
            toast.info("Created ticket successfully.");
            ticketListElement?.partialReset();
        }

        // Wait a little bit before enabling the button again
        // to prevent ugly flickering.
        setTimeout(() => {
            isFormLoading = false;
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

<TicketList bind:this={ticketListElement}
            bind:project={data.project}
            bind:sorting={data.sorting}
            bind:filter={data.filter}
            bind:tickets={data.tickets} />

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

    @media screen and (max-width: 980px)
        .bottom-row
            align-items: normal
            flex-direction: column
</style>
