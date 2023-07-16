<script>
    import PageLayout from "$lib/components/PageLayout.svelte";
    import SidebarGroup from "$lib/components/sidebar/SidebarGroup.svelte";
    import Sidebar from "$lib/components/sidebar/Sidebar.svelte";
    import Label from "$lib/components/GroupLabel.svelte";
    import SidebarEntry from "$lib/components/sidebar/SidebarEntry.svelte";
    import {ArrowLeft, Icon, User} from "svelte-hero-icons";
    import MainArea from "$lib/components/MainArea.svelte";
</script>

<PageLayout>
    <Sidebar>
        <SidebarGroup>
            <SidebarEntry src="/"
                          value="Back to Projects">
                <Icon src={ArrowLeft} />
            </SidebarEntry>
        </SidebarGroup>

        <Label value="User Settings" />
        <SidebarGroup>
            <SidebarEntry src="/user-settings"
                          value="Account">
                <Icon src={User} />
            </SidebarEntry>
        </SidebarGroup>
    </Sidebar>
    <MainArea>
        <slot />
    </MainArea>
</PageLayout>