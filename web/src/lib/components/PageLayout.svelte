<div id="wrapper">
    <slot />
</div>

<style lang="sass">
    @use "../../values"

    #wrapper
        display: grid
        grid-template-columns: 16.5em auto
        width: 100%
        max-height: 100%
        background-color: var(--background)
        border-left: var(--border)
        border-right: var(--border)
        overflow-x: hidden

    @media screen and (max-width: values.$max-width-for-hidden-sidebar)
        #wrapper
            grid-template-columns: auto
</style>
