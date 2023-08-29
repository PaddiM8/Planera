<script lang="ts">
    import { enhance } from "$app/forms";
    import ErrorText from "$lib/components/form/ErrorText.svelte";
    import type {ProblemDetails} from "$lib/problemDetails";
    import {onMount} from "svelte";
    import {beforeNavigate} from "$app/navigation";

    export let action: string | undefined = undefined;
    export let problem: ProblemDetails;
    export let beforeSubmit = undefined;
    export let afterSubmit = undefined;
    export let reset = true;
    export let horizontal = false;
    export let smallMargins = false;

    let form: HTMLFormElement;
    let isModified = false;
    let isSubmitting = false;

    beforeNavigate(({ cancel }) => {
        if (!isSubmitting && isModified && !confirm("Are you sure you want to leave this page? You have unsaved changes that will be lost.")) {
            cancel();
        }
    });

    function handleBeforeUnload() {
        if (!isSubmitting && isModified) {
            return true;
        }
    }

    onMount(() => {
        window.addEventListener("beforeunload", handleBeforeUnload);

        // Svelte events for the Editor component don't seem to bubble,
        // so we also need to listen to regular JavaScript events.
        form.addEventListener("input", () => isModified = true);

        return () => window.removeEventListener("beforeunload", handleBeforeUnload);
    });

    async function enhanceHandler(e) {
        isSubmitting = true;
        if (beforeSubmit) {
            await beforeSubmit(e);
        }

        return async ({ result, update }) => {
            await update({ reset });

            if (afterSubmit) {
                await afterSubmit(result.type === "success");
                setTimeout(() => {
                    isModified = false;
                }, 100);
            }

            isSubmitting = false;
        };
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.ctrlKey && e.key === "Enter") {
            e.preventDefault();
            e.stopPropagation();
            form.requestSubmit();
        }
    }
</script>

<form method="POST"
      {action}
      enctype="multipart/form-data"
      class:horizontal
      class:small-margins={smallMargins}
      bind:this={form}
      on:change={() => isModified = true}
      on:input={() => isModified = true}
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
    <div class="fields">
        <slot></slot>
    </div>
</form>

<style lang="sass">
    form
        display: flex
        flex-direction: column
        gap: var(--spacing)
        margin-top: calc(-1 * var(--spacing))

        &.horizontal .fields
            flex-direction: row

        &.small-margins
            margin-top: calc(-0.5 * var(--spacing))

            .fields
                gap: calc(var(--spacing) / 2)

    .errors
        display: flex
        flex-direction: column

    .fields
        display: flex
        flex-direction: column
        gap: var(--spacing)
</style>