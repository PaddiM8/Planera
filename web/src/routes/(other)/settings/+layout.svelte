<script>
    import PageLayout from "$lib/components/PageLayout.svelte";
    import SidebarGroup from "$lib/components/sidebar/SidebarGroup.svelte";
    import Sidebar from "$lib/components/sidebar/Sidebar.svelte";
    import Label from "$lib/components/GroupLabel.svelte";
    import SidebarEntry from "$lib/components/sidebar/SidebarEntry.svelte";
    import {ArrowLeft, Cube, Icon, User} from "svelte-hero-icons";
    import MainArea from "$lib/components/MainArea.svelte";
    import {onMount} from "svelte";
    import {startUserHub} from "$lib/hubs";
    import {userHub} from "./store";

    onMount(async () => {
        await createUserHub();
    });

    async function createUserHub() {
        const hub = await startUserHub();
        userHub.set(hub);
        hub.onreconnected(createUserHub);
    }
</script>

<PageLayout>
    <Sidebar>
        <SidebarGroup>
            <SidebarEntry src="/"
                          value="Back to Projects">
                <Icon src={ArrowLeft} />
            </SidebarEntry>
        </SidebarGroup>

        <Label value="Settings" />
        <SidebarGroup>
            <SidebarEntry src="/settings/general"
                          value="General">
                <Icon src={Cube} />
            </SidebarEntry>
            <SidebarEntry src="/settings/account"
                          value="Account">
                <Icon src={User} />
            </SidebarEntry>
        </SidebarGroup>
    </Sidebar>
    <MainArea>
        <slot />
    </MainArea>
</PageLayout>