<script lang="ts">
    import {startProjectHub} from "$lib/hubs";
    import {onMount} from "svelte";
    import {ProjectDto, UserDto} from "../../../../../gen/planeraClient";
    import {participants} from "../../../store";
    import {browser} from "$app/environment";
    import {toast} from "$lib/toast";
    import {HubConnectionState} from "@microsoft/signalr";
    import {projectHub} from "./store";

    export let data: {
        project: ProjectDto,
    };

    let previousProjectId: number | undefined = undefined;

    async function connectToProject(projectId) {
        if (!browser || !$projectHub || $projectHub.state !== HubConnectionState.Connected) {
            return;
        }

        if (previousProjectId == projectId)
            return;

        if (previousProjectId) {
            try {
                await $projectHub.invoke("leave", previousProjectId);
            } catch (ex) {
                console.log(ex);
            }
        }

        try {
            await $projectHub.invoke("join", projectId);
        } catch (ex) {
            console.log(ex);
            toast.error("Failed to connect to project.");
        }

        previousProjectId = projectId;
    }

    $: connectToProject(data?.project.id);
    $: if (data) {
        $participants = data.project.participants;
    }

    onMount(async () => {
        for (const participant of data.project.participants) {
            if (participant.id === data.project.author.id) {
                participant["removable"] = false;
            }
        }

        await createProjectHub();
    });

    async function createProjectHub() {
        const hub = await startProjectHub();
        projectHub.set(hub);
        hub.onreconnected(createProjectHub);
        hub.on("onAddParticipant", onAddParticipant);
        hub.on("onRemoveParticipant", onRemoveParticipant);
        previousProjectId = undefined;
        await connectToProject(data.project.id);
    }

    function onAddParticipant(participant: UserDto) {
        participants.update(x => [participant, ...x]);
    }

    function onRemoveParticipant(name: string) {
        participants.update(x => x.filter(x => x.username !== name));
    }
</script>

<slot />