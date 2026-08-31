<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import ListBox from "$lib/components/form/ListBox.svelte";
    import {toast} from "$lib/toast";
    import {dialog} from "$lib/dialog";
    import {participants} from "../../../../store";
    import {
        NotificationActionKind,
        NotificationKinds,
        NotificationThresholdUnit,
        NotificationTriggerDto,
        NotificationTriggerKind,
        type ProjectDto
    } from "../../../../../../gen/planeraClient";
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


    interface Props {
        data: {
            project: ProjectDto,
        };
        form: any;
    }

    let { data, form }: Props = $props();
    let notificationTriggers = $state<NotificationTriggerDto[]>([]);
    let project = $derived(data.project);

    $effect(() => {
        notificationTriggers = data.project?.notificationTriggers ?? [];
    });
    
    let deleteFormSlugValue: string = $state()!;

    let enableNotifications = $derived(data.project.me?.enabledNotificationKinds?.includes(NotificationKinds.Core)
        ? "true"
        : "false");
    // svelte-ignore state_referenced_locally
    let notifyDeadlines = project.me?.enabledNotificationKinds?.includes(NotificationKinds.DeadlineOtherTicket)
        ? "all-tickets"
        : project.me?.enabledNotificationKinds?.includes(NotificationKinds.DeadlineMyTicket) ? "my-tickets" : "none";

    async function handleAddParticipant(name: string): Promise<boolean> {
        try {
            await $projectHub!.invoke("invite", project.id, name);
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
            await $projectHub!.invoke("removeParticipant", project.id, name);
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

    function afterSubmitConfigureNotifications(success: boolean) {
        if (success) {
            toast.info("Updated notification settings successfully.");
        } else {
            toast.error("Failed to update notification settings.");
        }
    }
    
    function handleAddNotificationTrigger() {
        const notificationTrigger = {
            trigger: NotificationTriggerKind.TimeUntilDeadline,
            thresholdUnit: NotificationThresholdUnit.Days,
            threshold: "",
            action: NotificationActionKind.PushNotification,
        } as NotificationTriggerDto;

        notificationTriggers.push(notificationTrigger);
    }
    
    async function handleSaveNotificationTriggers() {
        try {
            await $projectHub!.invoke("setNotificationTriggers", project.id, notificationTriggers);
            toast.info("Saved notification triggers successfully.");
        } catch (ex) {
            console.log(ex);
            toast.error("Failed to save notification triggers.");
        }
    }
    
    function handleRemoveNotificationTrigger(notificationTrigger: NotificationTriggerDto) {
        const index = notificationTriggers?.indexOf(notificationTrigger);
        if (index !== undefined && index !== -1) {
            notificationTriggers = notificationTriggers!
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
                      entityName={project.name ?? ""}
                      src={getAvatarUrl(project.iconPath, "big")}
                      type="project" />
        <Input type="text"
               value={project.name}
               label="Name"
               name="name"
               placeholder="Project name..." />
        <Input type="text"
               value={project.description}
               label="Description"
               name="description"
               placeholder="Project description..." />

        <div class="group">
            <FormLabel value="Project Descriptions" />
            <MultiButton yesNo
                         selectedValue={project.enableTicketDescriptions ? "true" : "false"}
                         name="enableTicketDescriptions" />
        </div>

        <div class="group">
            <FormLabel value="Project Assignees" />
            <MultiButton yesNo
                         selectedValue={project.enableTicketAssignees ? "true" : "false"}
                         name="enableTicketAssignees" />
        </div>

        <div class="group">
            <FormLabel value="Deadlines" />
            <MultiButton yesNo
                         selectedValue={project.enableTicketDeadlines ? "true" : "false"}
                         name="enableTicketDeadlines" />
        </div>

        <div class="buttons">
            <a href="/projects/{project.author?.username}/{data.project.slug}">
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

<h2>Notifications for {project.me?.user.username}</h2>
<section class="notifications">
    <Form action="?/configureNotifications"
          problem={form?.problem}
          afterSubmit={afterSubmitConfigureNotifications}
          reset={false}>
        <input type="hidden" name="projectId" value={project.id} />
        <div>
            <FormLabel value="Enable notifications" />
            <MultiButton yesNo
                         name="enable-notifications"
                         bind:selectedValue={enableNotifications} />
        </div>

        <div>
            <FormLabel value="Notify me about deadlines" />
            <MultiButton choices={["None", "My Tickets", "All Tickets"]}
                         choiceValues={["none", "my-tickets", "all-tickets"]}
                         backgroundColors={["var(--severe)", "var(--normal)", "var(--normal)"]}
                         foregroundColors={["var(--on-severe)", "var(--on-normal)", "var(--on-normal)"]}
                         name="notify-deadlines"
                         selectedValue={notifyDeadlines}
                         disabled={enableNotifications == "false"} />
        </div>
        <Button value="Save" primary submit />
    </Form>
</section>

<hr>

<h2>Notification Triggers</h2>
<section class="notification-triggers">
    <div class="container">
        <Table headers={["Event", "Threshold", "Action", ""]}>
            {#each notificationTriggers as notificationTrigger (notificationTrigger)}
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
                        <button class="remove-button" onclick={() => handleRemoveNotificationTrigger(notificationTrigger)}>
                            <Icon src={Trash} size="1.2em" />
                        </button>
                    </TableCell>
                </TableRow>
            {/each}
        </Table>
    </div>
    <div class="buttons">
        <Button value="Add" onclick={handleAddNotificationTrigger} />
        <Button value="Save" primary onclick={handleSaveNotificationTriggers} />
    </div>
</section>

<hr>

<h2>Delete Project</h2>
<section class="delete">
    <Form action="?/delete"
          afterSubmit={() => goto("/")}
          validState={deleteFormSlugValue === project.slug}>
        <p>Type <strong>{project.slug}</strong> to confirm that you want to delete the project.</p>
        <input type="hidden" name="projectId" value={project.id} />
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
        
    .notification-triggers
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
                
    :global(.notification-triggers table)
        min-width: 50em

    .delete
        p
            margin: 0

        strong
            word-break: break-all
            font-style: normal
            font-weight: 500
</style>