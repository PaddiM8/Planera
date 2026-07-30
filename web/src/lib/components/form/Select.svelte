<script context="module">
    import { writable } from "svelte/store";
    let lastMenuId = writable(0);
</script>

<script lang="ts">
    import {Icon, ChevronDown} from "svelte-hero-icons";
    import {createEventDispatcher, onMount} from "svelte";

    export let choices: string[];
    export let selectedValue: string = "";
    export let selectedIndex: number | undefined = 0;
    export let width: string | undefined = undefined;
    export let name: string | undefined = undefined;

    const menuId = `menu-${$lastMenuId++}`;
    let menuElement: HTMLElement;
    const dispatcher = createEventDispatcher();

    if (selectedValue) {
        selectedIndex = choices.indexOf(selectedValue);
    }
    
    $: if (selectedIndex !== undefined) {
        selectedValue = choices[selectedIndex];
    }

    onMount(() => {
        if (!selectedValue && choices.length > 0) {
            selectedValue = choices[0];
        }
    });

    function handleItemClick(item: string) {
        selectedValue = item;
        selectedIndex = choices.indexOf(selectedValue);
        dispatcher("change", item);
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
            // Prevent scrolling
            e.preventDefault();
            
            const direction = e.key === "ArrowDown" ? 1 : -1;
            const currentIndex = choices.indexOf(selectedValue);
            const newIndex = Math.min(
                choices.length - 1,
                Math.max(0, currentIndex + direction)
            );
            selectedValue = choices[newIndex];
            selectedIndex = newIndex;
        }
    }
</script>

<div class="select">
    <input {name}
           inputmode="none"
           bind:value={selectedValue}
           on:focus={() => menuElement.showPopover()}
           on:blur={() => menuElement.hidePopover()}
           on:keydown={handleKeyDown}
           style="anchor-name: --anchor-{menuId}; {width ? `width: ${width};` : ''}" />
    <span class="icon">
        <Icon src={ChevronDown} />
    </span>
    <div id={menuId} class="menu" popover="manual" bind:this={menuElement} style="position-anchor: --anchor-{menuId}">
        {#each choices as choice}
            <button class="item"
                  class:selected={selectedValue === choice}
                  on:mousedown={() => handleItemClick(choice)}>{choice}</button>
        {/each}
    </div>
</div>

<style lang="sass">
    .select
        position: relative
        display: flex
        align-items: stretch
        caret-color: transparent

    input
        position: relative
        display: block
        width: 100%
        font-size: 1rem
        padding: var(--vertical-padding) var(--horizontal-padding)
        padding-right: 2em
        border-radius: var(--radius)
        font-weight: 500
        border: 0
        color: var(--on-background)
        background-color: var(--component-background)
        outline: var(--border)
        box-sizing: border-box
        cursor: pointer

        caret-color: transparent
        user-select: none
        -webkit-user-select: none
        -webkit-touch-callout: none
        -moz-user-select: none

        &::selection, &::-moz-selection
            background-color: transparent

        &:hover
            background-color: var(--background-hover)

        &:focus
            background-color: var(--background-hover)
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

    .menu
        position: fixed
        display: none
        flex-direction: column

        width: anchor-size(width)
        top: calc(anchor(bottom) + 0.25em)
        left: anchor(left)
        margin: 0
        padding: 0
        
        border-radius: var(--radius)
        border: var(--border)
        background-color: var(--background)
        color: var(--on-background)
        
        &:popover-open
            display: flex

        .item
            padding: var(--vertical-padding) var(--horizontal-padding)
            font-weight: 500
            font-size: 1em
            text-align: left
            color: var(--on-background)
            cursor: pointer

            &:hover, &.selected
                background-color: var(--background-hover)
</style>