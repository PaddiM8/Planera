<div id="wrapper">
    <slot />
</div>

<style lang="sass">
    #wrapper
        display: grid
        grid-template-columns: 20em auto
        width: 100%
        overflow-x: hidden

        border-left: var(--border)
        border-right: var(--border)
</style>
