<script lang="ts">
    import PageData = App.PageData;

    export let data: PageData;
</script>

<div id="content">
    <header>
        <span class="logo">Planera</span>
        {#if data?.user}
            <div class="user">
                <span class="username">{ data.user.username }</span>
            </div>
        {/if}
    </header>

    <main>
        <slot></slot>
    </main>
</div>

<style lang="sass">
@import "@fontsource-variable/inter"

:global(body)
    width: 100%
    height: 100%
    margin: 0
    padding: 0
    font-family: "Inter", sans-serif

:global(:root)
    --padding: 0.8em
    --border: 1px solid #D3D3D3

#content
    display: flex
    flex-direction: column

header
    display: flex
    align-items: center
    padding: var(--padding)
    border-bottom: var(--border)

    .logo
        font-size: 1.6em
        font-weight: 550
        margin: 0

    .user
        margin-left: auto
</style>