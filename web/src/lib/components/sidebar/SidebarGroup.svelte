<script lang="ts">
    interface Props {
        children?: import("svelte").Snippet;
    }

    let { children }: Props = $props();
</script>

<div class="group">
    {@render children?.()}
</div>

<style lang="sass">
    .group
        display: flex
        flex-direction: column
        margin-bottom: 1em
</style>