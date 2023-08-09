<script lang="ts">
    import { enhance } from "$app/forms";
    import ErrorText from "$lib/components/form/ErrorText.svelte";
    import type {ProblemDetails} from "$lib/problemDetails";

    export let action: string | undefined = undefined;
    export let problem: ProblemDetails;
    export let beforeSubmit = undefined;
    export let afterSubmit = undefined;
    export let reset = true;

    let form: HTMLFormElement;

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

    function handleKeyDown(e: KeyboardEvent) {
        if (e.ctrlKey && e.key === "Enter") {
            form.requestSubmit();
        }
    }
</script>

<form method="POST"
      {action}
      enctype="multipart/form-data"
      bind:this={form}
      on:keydown={handleKeyDown}
      use:enhance={enhanceHandler}>
    <div class="errors">
        {#if problem && (!problem?.errors || Object.keys(problem.errors).length === 0)}
            <ErrorText value={problem?.summary} />
        {/if}
        {#each Object.values(problem?.errors ?? {}) as error}
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