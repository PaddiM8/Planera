<!--suppress TypeScriptCheckImport -->
<script lang="ts">
    import {Icon, MinusCircle} from "svelte-hero-icons";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import type {MaybePromise} from "@sveltejs/kit";

    export let items: {}[] = [];
    export let key: string | undefined = undefined;
    export let canAdd: boolean = false;
    export let canRemove: boolean = false;
    export let placeholder: string = "";
    export let emptyText: string = "";
    export let addButtonText = "Add";
    export let handleAdd: (value: string) => MaybePromise<boolean>;
    export let handleRemove: (value: string) => MaybePromise<boolean>;

    let inputValue: string;
    let error: string;
    let addButton;

    async function handleClickAdd() {
        const success = await handleAdd(inputValue);
        if (success) {
            inputValue = "";
            error = "";
        }
    }

    async function handleClickRemove(item: any) {
        await handleRemove(getValue(item));
    }

    function getValue(item: any) {
        return key ? item[key] : item;
    }

    function isRemovable(item: any) {
        if (!key || !("removable" in item)) {
            return true;
        }

        return item["removable"];
    }
</script>

{#if canAdd}
    <div class="add-area">
        <Input {placeholder}
               submitButton={addButton}
               bind:value={inputValue} />
        <Button value={addButtonText}
                primary
                bind:this={addButton}
                on:click={handleClickAdd} />
    </div>
{/if}

<div class="list">
    {#if items.length === 0 && emptyText}
        <span class="item empty">
            <span class="text">{emptyText}</span>
        </span>
    {/if}
    {#each items as item}
        <span class="item">
            <span class="text">{getValue(item)}</span>
            {#if canRemove && isRemovable(item)}
                <span class="remove-button" on:click={() => handleClickRemove(item)}>
                    <Icon src={MinusCircle} />
                </span>
            {/if}
        </span>
    {/each}
</div>

<style lang="sass">
    .add-area
        width: 100%
        display: grid
        gap: var(--spacing)
        grid-template-columns: 1fr auto

    .list
        min-width: 6em
        min-height: 4em
        width: 100%
        height: 100%
        margin-top: var(--spacing)
        padding-bottom: 0.4em
        border-radius: var(--radius)
        border: var(--border)
        font-size: 1rem
        background-color: var(--background)
        overflow-y: auto

    .item
        display: flex
        padding: calc(var(--vertical-padding) + var(--border-width)) calc(var(--horizontal-padding) + var(--border-width))
        border-bottom: var(--border)
        border-top-left-radius: var(--radius)
        border-top-right-radius: var(--radius)

        background-color: var(--component-background)

        &.empty .text
            color: var(--text-gray)

        .text
            padding-right: 0.7em
            white-space: nowrap
            overflow: hidden
            text-overflow: ellipsis
            font-weight: 450

        .remove-button
            width: 1.2em
            height: 1.2em
            min-width: 1.2em
            min-height: 1.2em
            margin-left: auto
            cursor: pointer

            &:hover
                color: var(--red)
</style>