<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {createEventDispatcher} from "svelte";

    export let items: { value: string, image: string | undefined }[] = [];
    export let key: string | undefined = undefined;
    export let query: string = "";
    export let shown: boolean = true;
    export let showUserIcons: boolean = false;
    export let selectedIndex = 0;
    export let selectedValue: string | undefined = undefined;
    export let ignored: string[] = [];

    let shownItems = [];
    let previousIndex = 0;
    const dispatch = createEventDispatcher();

    function getValue(obj) {
        return key
            ? obj[key]
            : obj;
    }

    $: {
        if (items.length > 0) {
            selectedValue = getValue(items[selectedIndex]);
        }

        if (previousIndex == selectedIndex) {
            const indexedItems = items.map((x, i) => {
                return { ...x, index: i };
            });

            shownItems = indexedItems.filter(x =>
                !ignored.includes(getValue(x)) && getValue(x).includes(query)
            );
            selectedIndex = 0;
            previousIndex = selectedIndex;
        }
    }

    // Needs to be fired by a mousedown event in order to
    // make sure it fires *before* blur events. Parent
    // components may want to listen to on:blur to know
    // when the close the suggestion list, but the suggestion
    // list should get a chance to listen for clicks first.
    function handleItemClick(e) {
        query = "";
        dispatch("select", {
            index: e.target.getAttribute("data-index")
        });
    }
</script>

<div class="list" class:shown={shown}>
    {#each shownItems as item, i}
        <span class="item"
              data-index={item.index}
              class:selected={i === selectedIndex}
              on:mousedown={handleItemClick}>
            {#if showUserIcons}
                <span class="icon">
                    <UserIcon name={getValue(item)} image={item.image} type="user" />
                </span>
            {/if}
            <span class="value">{getValue(item)}</span>
        </span>
    {/each}
</div>

<style lang="sass">
    .list:empty, .list:not(.shown)
        display: none

    .list
        display: flex
        flex-direction: column
        position: absolute

        width: 100%
        max-height: 10em
        top: calc(100% + var(--border-width) * 2)
        left: 0

        border: var(--border)
        border-radius: var(--radius)
        overflow-y: auto
        box-sizing: border-box
        background-color: var(--background-secondary)

    .item
        display: flex
        padding: var(--vertical-padding) var(--horizontal-padding)
        border-bottom: var(--border)
        font-weight: 450
        user-select: none
        cursor: pointer

        &:last-child
            border-bottom: 0

        &:hover
            background-color: var(--background-hover)

        &.selected
            background-color: var(--background-hover)

        .icon
            width: 1.2em
            margin-left: -0.2em
            margin-right: 0.3em
</style>