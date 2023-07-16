<div id="wrapper">
    <slot />
</div>

<style lang="sass">
    #wrapper
        display: grid
        grid-template-columns: 20em auto
        width: 100%
        background-color: var(--background)
        border-left: var(--border)
        border-right: var(--border)
        overflow-x: hidden
</style>
