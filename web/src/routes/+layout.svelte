<script lang="ts">
    import PageData = App.PageData;
    import Icon from "$lib/Icon.svelte";

    export let data: PageData;
</script>

<div id="content">
    <header>
        <span class="logo">Planera</span>
        {#if data?.user}
            <div class="user">
                <span class="username">{ data.user.username }</span>
                <Icon type="user" name={ data.user.username } />
            </div>
        {/if}
    </header>

    <section class="page">
        <slot></slot>
    </section>
</div>

<style lang="sass">
    @import "@fontsource-variable/inter"

    :global(body)
        width: 100%
        height: 100%

        padding: 0
        margin: 0
        font-family: "Inter", sans-serif

    :global(:root)
        --background: white
        --primary: #1d4ed8
        --on-primary: #fafafa
        --primary-hover: #2563eb
        --button-background: #cbd5e1
        --on-button-background: #0a0a0a
        --hover-on-background: #d3d3d3
        --selected-on-background: #bbdefb
        --border-width: 1px
        --border: var(--border-width) solid #d6d3d1
        --text-gray: #757575
        --spacing: 0.8rem
        --vertical-padding: 0.55rem
        --horizontal-padding: 0.7rem
        --radius: 0.375rem

    :global(h1)
        font-size: 2.1em
        margin-bottom: 0.4em
        font-weight: 650

    #content
        display: grid
        grid-template-rows: auto 1fr
        min-height: 100vh

    header
        display: flex
        align-items: center
        padding: var(--spacing)
        border-bottom: var(--border)

        .logo
            font-size: 1.6em
            font-weight: 550
            margin: 0

        .user
            display: flex
            margin-left: auto
            gap: 0.4em

    .page
        display: flex
        max-width: 80em
        height: 100%

        margin-left: 50%
        transform: translateX(-50%)
</style>