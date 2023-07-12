<script lang="ts">
    import {ExclamationCircle, Icon, InformationCircle} from "svelte-hero-icons";
</script>

<div id="toast">
    <span class="icon info">
        <Icon src={InformationCircle} />
    </span>
    <span class="icon error">
        <Icon src={ExclamationCircle} />
    </span>
    <span class="text"></span>
</div>

<style lang="sass">
    :global(#toast:not(.shown))
        transform: translate(-50%, -120%)

    :global(#toast.shown)
        transform: translate(-50%, 0.325em)

    :global(#toast.info)
        background-color: #171717

        .icon.info
            display: block

    :global(#toast.error)
        background-color: #b91c1c

        .icon.error
            display: block

    #toast
        position: absolute
        display: flex
        align-items: center
        gap: 0.4em

        top: 0
        left: 50%
        padding: var(--vertical-padding) var(--horizontal-padding)

        border-radius: var(--radius)
        color: white
        box-shadow: 0 3px 2px -2px rgba(0, 0, 0, 0.4)
        -webkit-box-shadow: 0 3px 2px -2px rgba(0, 0, 0, 0.4)

        font-weight: 450
        z-index: 9999999
        transition: 250ms ease transform

        .icon
            display: none
            width: 1.5em
            height: 1.5em
            color: inherit
</style>