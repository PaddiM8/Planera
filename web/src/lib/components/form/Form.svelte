<script lang="ts">
    import { enhance } from "$app/forms";
    import ErrorText from "$lib/components/form/ErrorText.svelte";

    export let action: string | undefined = undefined;
    export let errors: { string: string[] } = [] as { string: string[] };
    export let beforeSubmit = undefined;
    export let afterSubmit = undefined;
    export let reset = true;

    async function enhanceHandler(e) {
        if (beforeSubmit) {
            await beforeSubmit(e);
        }

        return async ({ result, update }) => {
            await update({ reset });

            if (afterSubmit) {
                await afterSubmit(result.type === "success");
            }
        };
    }
</script>

<form method="POST" {action}
      enctype="multipart/form-data"
      use:enhance={enhanceHandler}>
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
        margin-top: calc(-1 * var(--spacing))

    .errors
        display: flex
        flex-direction: column
</style>