<script lang="ts">
    export let name: string;
    export let image: string | undefined = undefined;
    export let type: "user" | "project";
</script>

<span class="logo {type}">
    {#if image}
        <img class="image" src={image} alt={name} />
    {:else}
        <span class="letter">{ name[0] }</span>
    {/if}
</span>

<style lang="sass">
    .logo
        display: flex
        align-items: center
        justify-content: center

        width: 1.35em
        height: 1.35em
        border-radius: 100%

        &.user
            background-color: #64748b

        &.project
            background-color: #737373

        .letter
            font-size: 0.75em
            font-weight: 750
            color: white

</style>