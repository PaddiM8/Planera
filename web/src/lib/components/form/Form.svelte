<script lang="ts">
    import { enhance } from "$app/forms";
    import type {ProblemDetails} from "$lib/problemDetails";
    import {onMount} from "svelte";
    import {beforeNavigate} from "$app/navigation";
    import {browser} from "$app/environment";
    import type {SubmitFunction} from "@sveltejs/kit";
    import type {FormSubmitInput} from "../../../routes/types";

    interface Props {
        action?: string | undefined;
        problem?: ProblemDetails | undefined;
        beforeSubmit?: (input: FormSubmitInput) => void;
        afterSubmit?: (success: boolean) => void;
        reset?: boolean;
        horizontal?: boolean;
        smallMargins?: boolean;
        validState?: boolean;
        promptWhenModified?: boolean;
        refresh?: boolean;
        children?: import('svelte').Snippet;
    }

    let {
        action = undefined,
        problem = undefined,
        beforeSubmit = undefined!,
        afterSubmit = undefined!,
        reset = true,
        horizontal = false,
        smallMargins = false,
        validState = $bindable(true),
        promptWhenModified = false,
        refresh = true,
        children
    }: Props = $props();

    let form: HTMLFormElement | undefined = $state();
    let isModified = $state(false);
    let isSubmitting = $state(false);


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
        form?.addEventListener("input", () => isModified = true);

        return () => window.removeEventListener("beforeunload", handleBeforeUnload);
    });

    const enhanceHandler: SubmitFunction<Record<string, unknown>, Record<string, any>> = async e => {
        isSubmitting = true;

        // Trim all input values
        for (const [key, value] of e.formData.entries()) {
            if (typeof value === "string") {
                e.formData.set(key, value.trim());
            }
        }

        if (beforeSubmit) {
            await beforeSubmit(e as any);
        }

        return async ({ result, update }) => {
            if (refresh) {
                await update({ reset });
            }

            if (afterSubmit) {
                await afterSubmit(result.type === "success");
            }

            setTimeout(() => {
                if (result.type === "success") {
                    isModified = false;
                }
            }, 100);

            // Wait a little bit before enabling the button again
            // to prevent ugly flickering.
            setTimeout(() => {
                isSubmitting = false;
            }, 300);
        };
    }


    function showErrors(problem: ProblemDetails | undefined) {
        // Remove existing errors
        for (const error of form!.querySelectorAll(".form-error")) {
            (error as HTMLElement).parentElement?.removeChild(error);
        }

        if (!problem?.errors) {
            return;
        }

        for (const fieldName in problem!.errors) {
            const field = form!.querySelector(`[name="${fieldName}"]`);
            if (!field) {
                continue;
            }

            const errorElement = document.createElement("span");
            errorElement.className = "form-error";
            errorElement.innerHTML = problem.errors[fieldName].join("<br>");
            (field as HTMLElement).parentElement?.insertBefore(errorElement, field);
        }
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.ctrlKey && e.key === "Enter") {
            e.preventDefault();
            e.stopPropagation();
            form?.requestSubmit();
        }
    }

    $effect(() => {
        if (browser) {
            const primaryButton = form?.querySelector("button.primary, input.primary");
            const canSubmit = !isSubmitting && validState;
            if (primaryButton && canSubmit) {
                primaryButton.removeAttribute("disabled");
            } else if (primaryButton) {
                primaryButton.setAttribute("disabled", "");
            }
        }
    });

    $effect(() => {
        if (form && browser) {
            showErrors(problem);
        }
    });
</script>

<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
<form method="POST"
      {action}
      enctype="multipart/form-data"
      class:horizontal
      class:small-margins={smallMargins}
      bind:this={form}
      onchange={() => isModified = true}
      oninput={() => isModified = true}
      onkeydown={handleKeyDown}
      use:enhance={enhanceHandler}>
    <div class="fields">
        {@render children?.()}
    </div>
</form>

<style lang="sass">
    form
        display: flex
        flex-direction: column
        gap: var(--spacing)

        &.horizontal .fields
            flex-direction: row

        &.small-margins .fields
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