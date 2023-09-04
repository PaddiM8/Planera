<script lang="ts">
    export let name: string;
    export let image: string | undefined = undefined;
    export let type: "user" | "project";
</script>

<span class="logo {type}" class:image={image}>
    {#if image}
        <img class="image" src={image} alt={name} />
    {:else}
        <span class="letter">{ name?.at(0) ?? "?" }</span>
    {/if}
</span>

<style lang="sass">
    .logo
        display: flex
        align-items: center
        justify-content: center

        aspect-ratio: 1 / 1
        border-radius: 100%
        cursor: default
        user-select: none
        overflow: hidden

        &.image
            outline: var(--border)
            background-color: var(--background)

        &:not(.image).user
            background-color: #64748b

        &:not(.image).project
            background-color: #737373

        img
            width: 100%
            height: 100%
            object-fit: cover

        .letter
            font-size: 0.75em
            font-weight: 750
            color: white

</style>