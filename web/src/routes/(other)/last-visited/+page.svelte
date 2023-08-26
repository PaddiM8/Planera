<script lang="ts">
    import {onMount} from "svelte";

    onMount(() => {
        const lastVisited = localStorage.getItem("lastVisited");
        window.location.href = lastVisited || "/";
    });
</script>