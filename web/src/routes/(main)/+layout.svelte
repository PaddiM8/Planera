<script lang="ts">
    import {AuthenticationInfo, type ProjectDto} from "../../gen/planeraClient";
    import ErrorText from "$lib/components/form/ErrorText.svelte";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import Label from "$lib/components/GroupLabel.svelte";
    import {ListBullet, Icon, SquaresPlus, PlusSmall} from "svelte-hero-icons";
    import {onMount} from "svelte";
    import {startUserHub} from "$lib/hubs";
    import {invitations} from "./store";
    import {userHub} from "./store";
    import Sidebar from "$lib/components/sidebar/Sidebar.svelte";
    import SidebarEntry from "$lib/components/sidebar/SidebarEntry.svelte";
    import SidebarGroup from "$lib/components/sidebar/SidebarGroup.svelte";
    import PageLayout from "$lib/components/PageLayout.svelte";
    import MainArea from "$lib/components/MainArea.svelte";
    import {getAvatarUrl} from "$lib/clients";
    import {browser} from "$app/environment";
    import {subscribeToPushNotifications} from "$lib/notifications";

    interface Props {
        data: {
        projects: ProjectDto[],
        invitations: ProjectDto[],
        error: boolean,
        authenticationInfo: AuthenticationInfo,
    };
        children?: import('svelte').Snippet;
    }

    let { data = $bindable(), children }: Props = $props();

    onMount(async () => {
        invitations.set(data.invitations);

        if (browser) {
            const hub = await createUserHub();

            if (data.authenticationInfo.vapidPublicKey) {
                await subscribeToPushNotifications(hub, data.authenticationInfo.vapidPublicKey);
            }
        }
    });

    async function createUserHub() {
        const hub = await startUserHub();
        userHub.set(hub);
        hub.onreconnected(createUserHub);
        hub.on("onAddProject", onAddProject);
        hub.on("onAddInvitation", onAddInvitation);
        
        return hub;
    }

    function onAddProject(project: ProjectDto) {
        data.projects = [project, ...data.projects];
    }

    function onAddInvitation(project: ProjectDto) {
        invitations.update(x => [project, ...x]);
    }
    
    async function handleDrop(value: { startIndex: number, dropIndex: number }) {
        const project = data.projects[value.startIndex];
        delete data.projects[value.startIndex];
        data.projects.splice(value.dropIndex, 0, project);
        data.projects = [...data.projects].filter(x => x);
        
        await $userHub?.invoke("setPinnedProjects", data.projects.map(p => p.id));
    }
</script>

<PageLayout>
    <Sidebar>
        <Label value="General" />
        <SidebarGroup>
            <SidebarEntry src="/"
                          value="Overview">
                <Icon src={ListBullet} />
            </SidebarEntry>
            <SidebarEntry src="/invitations"
                          value="Invitations"
                          unreadCount={$invitations?.length}>
                <Icon src={SquaresPlus} />
            </SidebarEntry>
            <SidebarEntry src="/projects/new"
                          value="New Project">
                <Icon src={PlusSmall} />
            </SidebarEntry>
        </SidebarGroup>

        <Label value="Projects" />
        <SidebarGroup>
            {#if data.error}
                <ErrorText value="Error loading projects." />
            {/if}
            {#each data?.projects ?? [] as project}
                <SidebarEntry src="/projects/{project.author?.username}/{project.slug}"
                              value={project.name}
                              draggable
                              settingsSrc="/projects/{project.author?.username}/{project.slug}/settings"
                              ondrop={handleDrop}>
                    <UserIcon type="project"
                              name={project.name}
                              image={getAvatarUrl(project.iconPath, "small")} />
                </SidebarEntry>
            {/each}
        </SidebarGroup>
    </Sidebar>
    <MainArea>
        {@render children?.()}
    </MainArea>
</PageLayout>