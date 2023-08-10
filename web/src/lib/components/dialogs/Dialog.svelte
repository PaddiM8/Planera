<script lang="ts">
    export let id: string;
    export let title: string;

    function handleKeyDown(e: KeyboardEvent) {
        const dialog = e.target as HTMLElement;
        if (e.key === "Enter") {
            const submitButton = dialog.querySelector("button.primary") as HTMLButtonElement;
            submitButton.click();
        } else if (e.key === "Escape") {
            const closeButton = dialog.querySelector("button.close") as HTMLButtonElement;
            closeButton.click();
        }
    }
</script>

<div id="dialog-background"></div>
<div id={id} class="dialog" tabindex="0" on:keydown={handleKeyDown}>
    <h1>{title}</h1>
    <slot />
</div>

<style lang="sass">
    :global(#dialog-background.shown)
        display: block

    :global(#dialog-background:not(.shown))
        display: none

    :global(.dialog.shown)
        display: flex

    :global(.dialog:not(.shown))
        display: none

   :global(.buttons)
       display: flex
       gap: 0.4em
       justify-content: flex-end

   h1
       font-size: 1.75em

   .dialog
       position: absolute
       flex-direction: column
       gap: 0.4em
       top: 50%
       left: 50%
       transform: translate(-50%, -50%)
       width: 25em
       padding: calc(var(--horizontal-padding) * 2) calc(var(--horizontal-padding) * 2)
       border-radius: var(--radius)
       background-color: var(--background)
       color: var(--on-background)
       z-index: 99999

       &:focus
           outline: 0

   #dialog-background
       position: fixed
       content: ''
       top: 0
       left: 0
       width: 100vw
       height: 100vh
       background-color: black
       opacity: 0.7
       z-index: 9000
</style>
