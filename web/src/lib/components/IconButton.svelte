<script lang="ts">
    import {Icon} from "svelte-hero-icons";

    export let value: string | undefined = undefined;
    export let icon;
    export let color: "default" | "green" | "blue" | "red" = "default";
</script>

<button on:click class:has-value={value}>
    <span class="icon"
          class:green={color === "green"}
          class:blue={color === "blue"}
          class:red={color === "red"}>
        <Icon src={icon} />
    </span>
    {#if value}
        <span class="text">{value}</span>
    {/if}
</button>

<style lang="sass">
    button
        display: flex
        align-items: center
        background-color: transparent
        border: 0
        padding: 0.3em 0.4em
        border-radius: var(--radius)
        font-family: inherit
        font-weight: 500
        cursor: pointer

        &:hover
            background-color: var(--background-hover)

        &:not(.has-value)
            padding: 0

            .icon
                color: var(--on-background-inactive)

                &:hover
                    color: var(--on-background)

            &:hover
                background-color: transparent

        .icon
            display: block
            margin-right: 0.1em
            height: 1.4em
            min-width: 1.4em

            &.green
                color: var(--green)

            &.blue
                color: var(--blue)

            &.red
                color: var(--red)
</style>
