<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import ListBox from "$lib/components/form/ListBox.svelte";
    import {toast} from "$lib/toast";
    import {dialog} from "$lib/dialog";
    import {participants} from "../../../../store";
    import {NotificationTriggerDto, type ProjectDto} from "../../../../../../gen/planeraClient";
    import {projectHub} from "../store";
    import {getAvatarUrl} from "$lib/clients";
    import AvatarPicker from "$lib/components/form/AvatarPicker.svelte";
    import {goto} from "$app/navigation";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";
    import Table from "$lib/components/table/Table.svelte";
    import TableRow from "$lib/components/table/TableRow.svelte";
    import TableCell from "$lib/components/table/TableCell.svelte";
    import Select from "$lib/components/form/Select.svelte";
    import {Icon, Trash} from "svelte-hero-icons";
    import {onMount} from "svelte";

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

        try {
            await $projectHub!.invoke("removeParticipant", data.project.id, name);
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
    
    function handleAddNotificationTrigger() {
        data.project.notificationTriggers = [...data.project.notificationTriggers ?? [], new NotificationTriggerDto()];
    }
    
    async function handleSaveNotificationTriggers() {
        try {
            await $projectHub!.invoke("setNotificationTriggers", data.project.id, data.project.notificationTriggers);
            toast.info("Saved notification triggers successfully.");
        } catch (ex) {
            console.log(ex);
            toast.info("Failed to save notification triggers.");
        }
    }
    
    function handleRemoveNotificationTrigger(notificationTrigger: NotificationTriggerDto) {
        const index = data.project.notificationTriggers?.indexOf(notificationTrigger);
        if (index !== undefined && index !== -1) {
            data.project.notificationTriggers = data.project.notificationTriggers!
                .filter(t => t != notificationTrigger);
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

        <div class="group">
            <FormLabel value="Project Descriptions" />
            <MultiButton choices={["Enable", "Disable"]}
                         choiceValues={["true", "false"]}
                         selectedValue={data.project.enableTicketDescriptions ? "true" : "false"}
                         name="enableTicketDescriptions" />
        </div>

        <div class="group">
            <FormLabel value="Project Assignees" />
            <MultiButton choices={["Enable", "Disable"]}
                         choiceValues={["true", "false"]}
                         selectedValue={data.project.enableTicketAssignees ? "true" : "false"}
                         name="enableTicketAssignees" />
        </div>

        <div class="group">
            <FormLabel value="Deadlines" />
            <MultiButton choices={["Enable", "Disable"]}
                         choiceValues={["true", "false"]}
                         selectedValue={data.project.enableTicketDeadlines ? "true" : "false"}
                         name="enableTicketDeadlines" />
        </div>

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

<h2>Notifications</h2>
<section class="notifications">
    <div class="container">
        <Table headers={["Event", "Threshold", "Action", ""]}>
            {#each data.project.notificationTriggers ?? [] as notificationTrigger}
                <TableRow>
                    <TableCell>
                        <Select choices={["Time until deadline"]}
                                bind:selectedIndex={notificationTrigger.trigger} />
                    </TableCell>
                    <TableCell>
                        <div class="threshold">
                            <Input bind:value={notificationTrigger.threshold} />
                            <Select choices={["minutes", "hours", "days"]}
                                    width="8em"
                                    bind:selectedIndex={notificationTrigger.thresholdUnit} />
                        </div>
                    </TableCell>
                    <TableCell>
                        <Select choices={["Push Notification"]}
                                bind:selectedIndex={notificationTrigger.action} />
                    </TableCell>
                    <TableCell>
                        <button class="remove-button" on:click={() => handleRemoveNotificationTrigger(notificationTrigger)}>
                            <Icon src={Trash} size="1.2em" />
                        </button>
                    </TableCell>
                </TableRow>
            {/each}
        </Table>
    </div>
    <div class="buttons">
        <Button value="Add" on:click={handleAddNotificationTrigger} />
        <Button value="Save" primary on:click={handleSaveNotificationTriggers} />
    </div>
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
        
    .notifications
        max-width: unset
        
        .container
            max-height: 30em
            overflow: scroll
        
        .threshold
            display: flex
            gap: 0.8em
            max-width: 18em

        .buttons
            display: flex
            gap: 0.8em
            margin-top: 0.8em

        .remove-button
            color: var(--on-background)
            cursor: pointer

            &:hover
                color: var(--red)

    .delete
        p
            margin: 0

        strong
            word-break: break-all
            font-style: normal
            font-weight: 500
</style>