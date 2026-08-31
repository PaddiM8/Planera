<script lang="ts">
    interface Props {
        children?: import("svelte").Snippet;
    }

    let { children }: Props = $props();
</script>

<tr>
    {@render children?.()}
</tr>

<style lang="sass">
</style>
