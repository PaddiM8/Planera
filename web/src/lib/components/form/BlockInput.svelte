<script lang="ts">
    import SuggestionList from "$lib/components/form/SuggestionList.svelte";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import {getAvatarUrl} from "$lib/clients";

    export let placeholder: string | undefined = undefined;
    export let options: any[];
    export let showUserIcons: boolean = false;
    export let values: any[] = [];
    export let key: string | undefined = undefined;
    export let outputKey: string | undefined = undefined;
    export let label: string | undefined = undefined;
    export let name: string = "";

    export function reset() {
        values = [];
    }

    let blockAreaElement: HTMLElement;
    let inputElement: HTMLInputElement;
    let value: string = "";
    let isFocused: boolean = false;
    let selectedSuggestion: any = undefined;

    function getValue(obj) {
        return key
            ? obj[key]
            : obj;
    }

    function addBlock(value: any) {
        values = [...values, value];
    }

    function popBlock() {
        const children = blockAreaElement.children;
        if (children.length > 0) {
            values = values.slice(0, values.length - 1);
        }
    }

    function handleKeyDown(e: KeyboardEvent) {
        if (e.key == "Backspace" && !inputElement.value) {
            popBlock();
        }

        if (e.key != "Space" && e.key != "Enter" || !selectedSuggestion) {
            return;
        }

        if (!options.some(x => getValue(x) === inputElement.value)) {
            return;
        }

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
        values = values.filter(x => x != item);
    }

    function handleSelectedSuggestion(e: CustomEvent<{ index: number }>) {
        const selectedObject = options.at(e.detail.index);
        if (!selectedObject) {
            return
        }

        addBlock(selectedObject);
    }
</script>

{#if label}
    <label>{label}</label>
{/if}

<span class="wrapper"
      class:no-blocks={values.length === 0}
      bind:this={blockAreaElement}>
    {#each values as item}
        <span class="block" on:click={() => handleBlockClick(item)}>
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
               on:focus={handleFocus}
               on:blur={handleBlur}
               on:keydown={handleKeyDown} />
    </span>
    <SuggestionList query={value}
                    items={options}
                    key={key}
                    ignored={values}
                    shown={isFocused}
                    {showUserIcons}
                    bind:selectedValue={selectedSuggestion}
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
        padding: calc(var(--vertical-padding) / 2) calc(var(--horizontal-padding) / 2)
        margin-bottom: calc(var(--vertical-padding) / 2)
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
            background-color: black

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