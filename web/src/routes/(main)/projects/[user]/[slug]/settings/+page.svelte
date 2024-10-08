<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import ListBox from "$lib/components/form/ListBox.svelte";
    import {toast} from "$lib/toast";
    import {dialog} from "$lib/dialog";
    import {participants} from "../../../../store";
    import type {ProjectDto} from "../../../../../../gen/planeraClient";
    import {projectHub} from "../store";
    import {getAvatarUrl} from "$lib/clients";
    import AvatarPicker from "$lib/components/form/AvatarPicker.svelte";
    import {goto} from "$app/navigation";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";

    export let data: {
        project: ProjectDto,
    };

    export let form;

    let deleteFormSlugValue: string;

    async function handleAddParticipant(name: string): Promise<boolean> {
        try {
            await $projectHub!.invoke("invite", data.project.id, name);
            toast.info(`Invited user "${name}".`);

            return true;
        } catch {
            toast.error("Failed to invite user.");

            return false;
        }
    }

    async function handleRemoveParticipant(name: string): Promise<boolean> {
        const confirmation = await dialog.yesNo("Remove participant", `Are you sure you want to remove the user "${name}" from the project?`);
        if (!confirmation) {
            return false;
        }

        await $projectHub!.invoke("removeParticipant", data.project.id, name);
        try {
            toast.info(`Removed user "${name}".`);

            return true;
        } catch {
            toast.info("Failed to remove user.");

            return false;
        }
    }

    function handleSubmit(success: boolean) {
        if (success) {
            toast.info("Project updated successfully.");
        }
    }
</script>

<svelte:head>
    <title>Project Settings - Planera</title>
</svelte:head>

<h1>Project Settings</h1>

<h2>About</h2>
<section class="about">
    <Form action="?/update"
          problem={form?.problem}
          afterSubmit={handleSubmit}
          promptWhenModified
          reset={false}>
        <AvatarPicker name="icon"
                      entityName={data.project.name ?? ""}
                      src={getAvatarUrl(data.project.iconPath, "big")}
                      type="project" />
        <Input type="text"
               value={data.project.name}
               label="Name"
               name="name"
               placeholder="Project name..." />
        <Input type="text"
               value={data.project.description}
               label="Description"
               name="description"
               placeholder="Project description..." />
        <FormLabel value="Project Descriptions" />
        <MultiButton choices={["Enable", "Disable"]}
                     choiceValues={["true", "false"]}
                     selectedValue={data.project.enableTicketDescriptions ? "true" : "false"}
                     name="enableTicketDescriptions" />
        <FormLabel value="Project Assignees" />
        <MultiButton choices={["Enable", "Disable"]}
                     choiceValues={["true", "false"]}
                     selectedValue={data.project.enableTicketAssignees ? "true" : "false"}
                     name="enableTicketAssignees" />
        <div class="buttons">
            <a href="/projects/{data.project.author?.username}/{data.project.slug}">
                <Button value="Cancel" />
            </a>
            <Button value="Update" primary submit />
        </div>
    </Form>
</section>

<hr>

<h2>Participants</h2>
<section class="participants">
    <ListBox items={$participants}
             key="username"
             canAdd
             canRemove
             placeholder="Invite someone..."
             emptyText="No participants."
             addButtonText="Invite"
             handleAdd={handleAddParticipant}
             handleRemove={handleRemoveParticipant} />
</section>

<hr>

<h2>Delete Project</h2>
<section class="delete">
    <Form action="?/delete"
          afterSubmit={() => goto("/")}
          validState={deleteFormSlugValue === data.project.slug}>
        <p>Type <strong>{data.project.slug}</strong> to confirm that you want to delete the project.</p>
        <input type="hidden" name="projectId" value={data.project.id} />
        <Input placeholder="Project slug..." bind:value={deleteFormSlugValue} />
        <Button value="Delete" danger primary submit />
    </Form>
</section>

<style lang="sass">
    section
        max-width: 35em

    .about
        .buttons
            display: flex
            gap: var(--spacing)
            margin-left: auto

    .participants
        height: 11.5em

    :global(.participants > *)
        height: 100%

    .delete
        p
            margin: 0

        strong
            word-break: break-all
            font-style: normal
            font-weight: 500
</style>