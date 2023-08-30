<div id="content">
    <slot />
</div>

<style lang="sass">
    #content
        width: 100%
        max-width: 25rem
        padding: 1.5em 2em
        margin: auto
        transform: translateY(-5em)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

    @media screen and (max-width: 580px)
        :global(.page)
            background-color: var(--component-background)

        #content
            max-width: none
            border: 0
            border-radius: 0
            transform: none
</style>
