<script lang="ts">
    import SuggestionList from "$lib/components/form/SuggestionList.svelte";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {getAvatarUrl} from "$lib/clients";

    interface Props {
        placeholder?: string | undefined;
        options: any[];
        showUserIcons?: boolean;
        values?: any[];
        key?: string | undefined;
        outputKey?: string | undefined;
        label?: string | undefined;
        name?: string;
        onadd?: (value: any) => void;
        onremove?: (value: any) => void;
    }

    let {
        placeholder = undefined,
        options,
        showUserIcons = false,
        values = $bindable([]),
        key = undefined,
        outputKey = undefined,
        label = undefined,
        name = "",
        onadd = undefined,
        onremove = undefined,
    }: Props = $props();

    export function reset() {
        values = [];
    }

    let blockAreaElement: HTMLElement = $state()!;
    let inputElement: HTMLInputElement = $state()!;
    let value: string = $state("");
    let isFocused: boolean = $state(false);
    let selectedSuggestion: any = $state(undefined);
    let suggestionList: SuggestionList = $state()!;

    function getValue(obj: any) {
        return key
            ? obj[key]
            : obj;
    }

    function addBlock(value: any) {
        values = [...values, value];
        if (onadd) {
            onadd(value);
        }
    }

    function popBlock() {
        const children = blockAreaElement.children;
        if (children.length > 0) {
            if (onremove) {
                onremove(values[values.length - 1]);
            }
            values = values.slice(0, values.length - 1);
        }
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.key == "Backspace" && !inputElement.value) {
            popBlock();
        }

        if (e.key === "ArrowDown") {
            suggestionList.selectNext();
            return;
        }

        if (e.key === "ArrowUp") {
            suggestionList.selectPrevious();
            return;
        }

        if (e.key != "Space" && e.key != "Enter" || !selectedSuggestion) {
            return;
        }

        value = "";
        addBlock(selectedSuggestion);
        e.preventDefault();
    }

    function handleFocus() {
        isFocused = true;
    }

    function handleBlur() {
        isFocused = false;
    }

    function handleBlockClick(item: any) {
        values = values.filter(x => {
            if (x == item && onremove) {
                onremove(item);
            }

            return x != item;
        });
    }

    function handleSelectedSuggestion(e: CustomEvent<{ value: any }>) {
        if (e.detail.value) {
            addBlock(e.detail.value);
        }
    }
</script>

{#if label}
    <label>{label}</label>
{/if}

<span class="wrapper"
      class:no-blocks={values.length === 0}
      bind:this={blockAreaElement}>
    {#each values as item}
        <span class="block" onclick={() => handleBlockClick(item)}>
            {#if showUserIcons}
                <span class="icon">
                    <UserIcon name={getValue(item)}
                              image={getAvatarUrl(item["avatarPath"], "small")}
                              type="user" />
                </span>
            {/if}
            <span class="value">{getValue(item)}</span>
            <input type="text"
                   name={name}
                   value={outputKey ? item[outputKey] : getValue(item)}
                   hidden />
        </span>
    {/each}
    <span class="input-area">
        <input type="text"
               class="input"
               size="1"
               {placeholder}
               bind:this={inputElement}
               bind:value={value}
               onfocus={handleFocus}
               onblur={handleBlur}
               onkeydown={handleKeyDown} />
    </span>
    <SuggestionList bind:query={value}
                    items={options}
                    key={key}
                    bind:ignored={values}
                    shown={isFocused}
                    {showUserIcons}
                    bind:selectedValue={selectedSuggestion}
                    bind:this={suggestionList}
                    on:select={handleSelectedSuggestion} />
</span>

<style lang="sass">
    $block-padding: calc(var(--vertical-padding) / 2)

    label
        margin-bottom: -0.4em

    .wrapper
        position: relative
        display: flex
        flex-wrap: wrap
        padding-top: calc(var(--vertical-padding) / 2)
        padding-bottom: calc(var(--vertical-padding) / 2)
        padding-left: calc(var(--vertical-padding) / 2)
        border-radius: var(--radius)
        border: 0
        outline: var(--border)
        background-color: var(--component-background)
        box-sizing: border-box

        &.no-blocks
            padding: calc(var(--vertical-padding) / 2) calc(var(--horizontal-padding) / 2)

    .block
        position: relative
        display: inline-flex
        align-items: center
        padding: calc(var(--vertical-padding) / 2) calc(var(--horizontal-padding) / 2)
        margin-right: calc(var(--vertical-padding) / 2)
        border-radius: var(--radius)
        background-color: var(--background-selected)
        cursor: pointer

        &:hover::before
            position: absolute
            display: block
            content: ''
            top: 0
            left: 0
            width: 100%
            height: 100%
            border-radius: var(--radius)
            background-color: white
            opacity: 0.25

        &:hover::after
            position: absolute
            display: block
            content: ''
            top: 50%
            left: 0.5em
            transform: translateY(-50%)
            width: calc(100% - 1em)
            height: 2px
            background-color: var(--on-background)

        .icon
            width: 1.2em
            height: 1.2em
            margin-right: 0.3em
            flex-grow: 1

    .input-area
        display: inline-flex
        flex-grow: 1
        overflow: hidden

    .input
        font-size: 1rem
        flex-grow: 1
        padding: calc(var(--vertical-padding) / 2) calc(var(--horizontal-padding) / 2)
        border: 0
        min-width: 5em
        color: var(--on-background)
        background-color: transparent
        box-sizing: border-box

        &:focus
            outline: 0

    .wrapper:focus-within
        outline-width: 2px
        outline-offset: -2px
        outline-color: var(--primary)

    .wrapper:not(.no-blocks) input::placeholder
        color: transparent
</style>
