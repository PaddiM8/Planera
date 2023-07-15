<script lang="ts">
    import type {ProjectDto} from "../../gen/planeraClient";
    import {page} from "$app/stores";
    import ErrorText from "$lib/components/form/ErrorText.svelte";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import Label from "$lib/components/form/Label.svelte";
    import {ListBullet, Icon, SquaresPlus} from "svelte-hero-icons";
    import {onMount} from "svelte";
    import {startUserHub} from "$lib/hubs";
    import {invitations} from "./store";
    import {userHub} from "./store";
    import Sidebar from "$lib/components/sidebar/Sidebar.svelte";
    import SidebarEntry from "$lib/components/sidebar/SidebarEntry.svelte";
    import SidebarGroup from "$lib/components/sidebar/SidebarGroup.svelte";
    import PageLayout from "$lib/components/PageLayout.svelte";
    import MainArea from "$lib/components/MainArea.svelte";

    export let data: {
        projects: ProjectDto[],
        invitations: ProjectDto[],
        error: boolean,
    };

    onMount(async () => {
        invitations.set(data.invitations);

        userHub.set(await startUserHub());
        $userHub?.on("onAddProject", onAddProject);
        $userHub?.on("onAddInvitation", onAddInvitation);
    });

    function onAddProject(project: ProjectDto) {
        data.projects = [project, ...data.projects];
    }

    function onAddInvitation(project: ProjectDto) {
        invitations.update(x => [project, ...x]);
    }
</script>

<PageLayout id="wrapper">
    <Sidebar>
        <Label value="General" />
        <SidebarGroup>
            <SidebarEntry src="/"
                          value="Overview">
                <Icon src={ListBullet} />
            </SidebarEntry>
            <SidebarEntry src="/invitations"
                          value="Invitations"
                          unreadCount={$invitations.length}>
                <Icon src={SquaresPlus} />
            </SidebarEntry>
        </SidebarGroup>

        <Label value="Projects" />
        <SidebarGroup>
            {#if data.error}
                <ErrorText value="Error loading projects." />
            {/if}
            {#each data?.projects ?? [] as project}
                <SidebarEntry src="/projects/{project.author.userName}/{project.slug}"
                              value={project.name}
                              settingsSrc="/projects/{project.author.userName}/{project.slug}/settings">
                    <UserIcon type="project" name="{project.name}" />
                </SidebarEntry>
            {/each}
        </SidebarGroup>
    </Sidebar>
    <MainArea>
        <slot />
    </MainArea>
</PageLayout>