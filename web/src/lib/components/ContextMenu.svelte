<script lang="ts">
    export let target: HTMLElement | undefined;

    let element: HTMLElement;
    let componentWidth: number = 0;
    let canBeClosed = true;

    $: {
        if (target && element) {
            const rect = target.getBoundingClientRect();
            const top = rect.top + rect.height;
            const left = rect.left + componentWidth > window.innerWidth
                ? window.innerWidth - componentWidth
                : rect.left;
            element.style.top = top + "px";
            element.style.left = left + "px";
            canBeClosed = false;
            setTimeout(() => canBeClosed = true, 250);
        }
    }

    function handlePageClick(e) {
        const targetIsOutside = e.target !== element && !element.contains(e.target);
        if (canBeClosed && (targetIsOutside || window.getComputedStyle(e.target).cursor === "pointer")) {
            target = undefined;
        }
    }
</script>

<div class="menu"
     bind:this={element}
     bind:offsetWidth={componentWidth}
     class:visible={target !== undefined}>
    <slot />
</div>

<svelte:body on:click={handlePageClick} />

<style lang="sass">
    :global(.label)
        margin-bottom: 0.2em

    .menu
        position: absolute
        display: flex
        flex-direction: column
        width: fit-content
        min-width: 11em
        padding: 0.3em
        box-sizing: border-box
        transform: translate(-0.2em, 0.2em)

        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--background)
        visibility: hidden
        z-index: 999999

        &.visible
            visibility: visible
</style>
