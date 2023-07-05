<script lang="ts">
    import { enhance } from "$app/forms";
    import ErrorText from "$lib/ErrorText.svelte";

    export let errors: { string: string[] } = [] as { string: string[] };
</script>

<form method="POST" use:enhance>
    <div class="errors">
        {#each Object.values(errors ?? {}) as error}
            <ErrorText value={error} />
        {/each}
    </div>
    <slot></slot>
</form>

<style lang="sass">
    form
        display: flex
        flex-direction: column
        gap: var(--spacing)

    .errors
        display: flex
        flex-direction: column
</style>