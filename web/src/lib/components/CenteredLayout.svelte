<div id="content">
    <slot />
</div>

<style lang="sass">
    #content
        width: 100%
        max-width: 25rem
        margin-left: 50%
        margin-top: 50vh
        transform: translate(-50%, -50%)

    @media screen and (max-width: 580px)
        #content
            max-width: none
            margin: 2em
            transform: none
</style>
