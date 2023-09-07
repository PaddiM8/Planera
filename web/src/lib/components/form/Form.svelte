<script lang="ts">
    import { enhance } from "$app/forms";
    import ErrorText from "$lib/components/form/ErrorText.svelte";
    import type {ProblemDetails} from "$lib/problemDetails";
    import {onMount} from "svelte";
    import {beforeNavigate} from "$app/navigation";
    import {browser} from "$app/environment";

    export let action: string | undefined = undefined;
    export let problem: ProblemDetails | undefined = undefined;
    export let beforeSubmit = undefined;
    export let afterSubmit = undefined;
    export let reset = true;
    export let horizontal = false;
    export let smallMargins = false;
    export let validState = true;
    export let promptWhenModified = false;
    export let refresh = true;

    let form: HTMLFormElement;
    let isModified = false;
    let isSubmitting = false;

    $: {
        if (browser) {
            const primaryButton = form?.querySelector("button.primary, input.primary");
            const canSubmit = !isSubmitting && validState;
            if (primaryButton && canSubmit) {
                primaryButton.removeAttribute("disabled");
            } else if (primaryButton) {
                primaryButton.setAttribute("disabled", "");
            }
        }
    }

    beforeNavigate(({ cancel }) => {
        if (promptWhenModified &&
            !isSubmitting &&
            isModified &&
            !confirm("Are you sure you want to leave this page? You have unsaved changes that will be lost.")) {
            cancel();
        }
    });

    function handleBeforeUnload() {
        if (promptWhenModified && !isSubmitting && isModified) {
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

        // Trim all input values
        for (const [key, value] of e.formData.entries()) {
            if (typeof value === "string") {
                e.formData.set(key, value.trim());
            }
        }

        if (beforeSubmit) {
            await beforeSubmit(e);
        }

        return async ({ result, update }) => {
            if (refresh) {
                await update({ reset });
            }

            if (afterSubmit) {
                await afterSubmit(result.type === "success");
                setTimeout(() => {
                    isModified = false;
                }, 100);
            }

            // Wait a little bit before enabling the button again
            // to prevent ugly flickering.
            setTimeout(() => {
                isSubmitting = false;
            }, 300);
        };
    }

    $: {
        if (problem && browser) {
            showErrors();
        }
    }

    function showErrors() {
        // Remove existing errors
        for (const error of form.querySelectorAll(".form-error")) {
            error.parentElement.removeChild(error);
        }

        if (!problem?.errors) {
            return;
        }

        for (const fieldName in problem!.errors) {
            const field = form.querySelector(`[name="${fieldName}"]`);
            if (!field) {
                continue;
            }

            const errorElement = document.createElement("span");
            errorElement.className = "form-error";
            errorElement.innerHTML = problem.errors[fieldName].join("<br>");
            field.parentElement.insertBefore(errorElement, field);
        }
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
    <div class="fields">
        <slot></slot>
    </div>
</form>

<style lang="sass">
    form
        display: flex
        flex-direction: column
        gap: var(--spacing)
        margin-top: calc(-0.75 * var(--spacing))

        &.horizontal .fields
            flex-direction: row

        &.small-margins
            margin-top: calc(-0.5 * var(--spacing))

            .fields
                gap: calc(var(--spacing) / 2)

    :global(.form-error)
        display: block
        color: var(--red)
        margin-bottom: 0.3em

    .fields
        display: flex
        flex-direction: column
        gap: var(--spacing)
</style>