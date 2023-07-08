<script lang="ts">
    import type {Project} from "../../../../../gen/planeraClient";
    import Form from "$lib/components/Form.svelte";
    import Input from "$lib/components/Input.svelte";
    import Button from "$lib/components/Button.svelte";
    import Editor from "$lib/components/editor/Editor.svelte";
    import MultiButton from "$lib/components/MultiButton.svelte";
    import Label from "$lib/components/Label.svelte";
    import BlockInput from "$lib/components/BlockInput.svelte";

    export let data: {
        project: Project | undefined,
        error: boolean,
    };

    let projectUsers = [
        { value: "user1" },
        { value: "user2" },
        { value: "user3" },
        { value: "user4" },
        { value: "user5" },
        { value: "user6" },
    ];

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

<section class="description">
    <h1>{ data.project.name }</h1>
    <h3>A short description of the project.</h3>
</section>

<section class="new-ticket">
    <h2>New Ticket</h2>
    <Form {beforeSubmit} {afterSubmit}>
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
                            options={projectUsers}
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
</section>

<style lang="sass">
    section
        border-bottom: var(--border)
        margin-bottom: 1em
        padding-bottom: 1em

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