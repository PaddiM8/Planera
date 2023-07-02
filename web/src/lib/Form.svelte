<script lang="ts">
    import { enhance } from "$app/forms";

    export let errors: { string: string[] };
</script>

<form method="POST" use:enhance>
    <div class="errors">
        {#each Object.values(errors ?? {}) as error}
            <span class="error">{ error }</span>
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

    .error
        color: red
        margin-bottom: 0.2em
</style>