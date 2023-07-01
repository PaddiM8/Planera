<script lang="ts">
    export let errors: { string: Array<string> } | undefined;
</script>

<div class="errors">
    {#each Object.values(errors ?? {}) as error}
        <span class="error">{ error }</span>
    {/each}
</div>

<style lang="sass">
    .errors
        display: flex

    .error
        color: red
        margin-bottom: 0.2em
</style>