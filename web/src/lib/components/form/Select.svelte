<script lang="ts">
    import {Icon, ChevronDown} from "svelte-hero-icons";
    import {createEventDispatcher, onMount} from "svelte";

    export let choices: string[];
    export let selectedValue: string = "";
    export let name: string | undefined = undefined;

    let isOpen = false;
    const dispatcher = createEventDispatcher();

    onMount(() => {
        if (!selectedValue && choices.length > 0) {
            selectedValue = choices[0];
        }
    });

    function handleItemClick(item: string) {
        selectedValue = item;
        dispatcher("change", item);
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
            const direction = e.key === "ArrowDown" ? 1 : -1;
            const currentIndex = choices.indexOf(selectedValue);
            const newIndex = Math.min(
                choices.length - 1,
                Math.max(0, currentIndex + direction)
            );
            selectedValue = choices[newIndex];
        }
    }
</script>

<div class="select" class:open={isOpen}>
    <input type="text"
           {name}
           readonly
           bind:value={selectedValue}
           on:focus={() => isOpen = true}
           on:blur={() => isOpen = false}
           on:keydown={handleKeyDown} />
    <span class="icon">
        <Icon src={ChevronDown} />
    </span>
    <div class="menu">
        {#each choices as choice}
            <span class="item"
                  class:selected={selectedValue === choice}
                  on:mousedown={() => handleItemClick(choice)}>{choice}</span>
        {/each}
    </div>
</div>

<style lang="sass">
    .select
        position: relative
        display: flex

        &.open input
            background-color: var(--background-hover)

    input
        position: relative
        display: block
        width: 100%
        font-size: 1rem
        padding: var(--vertical-padding) var(--horizontal-padding)
        padding-right: 2em
        border-radius: var(--radius)
        border: 0
        background-color: var(--component-background)
        outline: var(--border)
        box-sizing: border-box
        cursor: pointer

        &::selection, &::-moz-selection
            background-color: transparent

        &:hover
            background-color: var(--background-hover)

        &:focus
            outline-width: 2px
            outline-color: var(--primary)

    .icon
        position: absolute
        display: block
        right: 0
        top: 50%
        width: 1.2em
        height: 1.2em
        transform: translate(-50%, -50%)
        pointer-events: none

    .select:not(.open) .menu
        display: none

    .menu
        position: absolute
        display: flex
        flex-direction: column
        width: 100%
        bottom: 0
        left: 0
        transform: translateY(calc(100% + 0.25em))
        border-radius: var(--radius)
        border: var(--border)
        background-color: var(--background)
        overflow: hidden
        z-index: 999999

        .item
            padding: var(--vertical-padding) var(--horizontal-padding)
            font-weight: 500
            cursor: pointer

            &:hover, &.selected
                background-color: var(--background-hover)
</style>