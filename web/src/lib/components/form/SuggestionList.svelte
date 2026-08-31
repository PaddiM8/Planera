<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {createEventDispatcher} from "svelte";
    import {getAvatarUrl} from "$lib/clients";

    interface Props {
        items?: any[];
        key?: string | undefined;
        query?: string;
        shown?: boolean;
        showUserIcons?: boolean;
        selectedIndex?: number;
        selectedValue?: any | undefined;
        ignored?: any[];
    }

    let {
        items = [],
        key = undefined,
        query = $bindable(""),
        shown = true,
        showUserIcons = false,
        selectedIndex = $bindable(0),
        selectedValue = $bindable(undefined),
        ignored = $bindable([])
    }: Props = $props();

    let shownItems: any[] = $state([]);
    const dispatch = createEventDispatcher();

    function getValue(obj: any) {
        return key
            ? obj[key]
            : obj;
    }

    $effect(() => {
        if (shownItems.length > 0) {
            selectedValue = shownItems[selectedIndex];
        }
    });

    $effect(() => {
        const indexedItems = items.map((x, i) => {
            return { ...x, index: i };
        });

        shownItems = indexedItems.filter(x =>
            !ignored.some(y => y[key ?? ""] === x[key ?? ""]) && getValue(x).includes(query)
        );
        selectedIndex = 0;
    });

    export function selectNext() {
        selectedIndex = Math.min(shownItems.length - 1, selectedIndex + 1);
    }

    export function selectPrevious() {
        selectedIndex = Math.max(0, selectedIndex - 1);
    }

    // Needs to be fired by a mousedown event in order to
    // make sure it fires *before* blur events. Parent
    // components may want to listen to on:blur to know
    // when the close the suggestion list, but the suggestion
    // list should get a chance to listen for clicks first.
    function handleItemClick(index: number) {
        query = "";
        dispatch("select", {
            value: shownItems[index],
        });
    }
</script>

<div class="list" class:shown={shown}>
    {#each shownItems as item, i}
        <span class="item"
              data-index={i}
              class:selected={i === selectedIndex}
              onmousedown={() => handleItemClick(i)}
              role="button"
              tabindex="0">
            {#if showUserIcons}
                <span class="icon">
                    <UserIcon name={getValue(item)}
                              image={getAvatarUrl(item["avatarPath"], "small")}
                              type="user" />
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
        align-items: center
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
