<script lang="ts">
    import {Cog, Icon} from "svelte-hero-icons";
    import {page} from "$app/stores";
    import {invitations} from "../../../routes/(main)/store";
    import {createEventDispatcher} from "svelte";

    interface Props {
        src: string;
        value: string;
        unreadCount?: number;
        draggable?: boolean;
        settingsSrc?: string | undefined;
        children?: import('svelte').Snippet;
    }

    let {
        src,
        value,
        unreadCount = 0,
        draggable = false,
        settingsSrc = undefined,
        children
    }: Props = $props();

    const dispatch = createEventDispatcher();
    
    let outerElement: HTMLElement = $state();
    let isDragging = false;

    function withTrailingSlash(value: string) {
        return value?.endsWith("/")
            ? value
            : `${value}/`;
    }

    let path = $derived(withTrailingSlash($page.url?.pathname))
    
    function handleDragStart() {
        isDragging = true;
    }
    
    function handleDragEnd() {
        let startIndex = -1;
        let dropIndex = -1;
        for (let siblingIndex = 0; siblingIndex < outerElement.parentElement!.children.length; siblingIndex++) {
            const sibling = outerElement.parentElement!.children[siblingIndex];
            if (sibling == outerElement) {
                startIndex = siblingIndex;
                continue;
            }
            
            if (sibling.classList.contains("drag-active-top")) {
                dropIndex = siblingIndex;
            } else if (sibling.classList.contains("drag-active-bottom")) {
                dropIndex = siblingIndex + 1;
            }
        }

        if (dropIndex != -1) {
            dispatch("drop", {
                startIndex: startIndex,
                dropIndex: dropIndex,
            });
        }

        isDragging = false;
        outerElement.classList.remove("drag-active-top");
        outerElement.classList.remove("drag-active-bottom");
        clearSiblings();
    }
    
    function handleDragOver(event: DragEvent) {
        // If isDragging is true, that means *this* entry is the one
        // being dragged
        if (isDragging || !draggable) {
            return;
        }

        const target = event.target as HTMLElement;
        if (event.offsetY > target.offsetHeight / 2) {
            if (!outerElement.nextElementSibling?.classList.contains("drag-active-top")) {
                outerElement.classList.add("drag-active-bottom");
                clearSiblings();
            }

            outerElement.classList.remove("drag-active-top");
        } else {
            if (!outerElement.previousElementSibling?.classList.contains("drag-active-bottom")) {
                outerElement.classList.add("drag-active-top");
                clearSiblings();
            }

            outerElement.classList.remove("drag-active-bottom");
        }
    }
    
    function clearSiblings() {
        for (const sibling of outerElement.parentElement!.children) {
            if (sibling != outerElement) {
                sibling.classList.remove("drag-active-top");
                sibling.classList.remove("drag-active-bottom");
            }
        }
    }

    function handleDragLeave() {
        setTimeout(() => {
            outerElement.classList.remove("drag-active-top");
            outerElement.classList.remove("drag-active-bottom");
        }, 100);
    }
</script>

<div class="outer" bind:this={outerElement}>
    <a class="entry"
       href={src}
       draggable={draggable}
       ondragstart={handleDragStart}
       ondragend={handleDragEnd}
       ondragover={handleDragOver}
       ondragleave={handleDragLeave}
       class:selected={(withTrailingSlash(src) !== "/" && path.startsWith(withTrailingSlash(src)) || (src === "/" && path === "/"))}>
        <span class="icon">
            {@render children?.()}
        </span>
        <span class="name">{value}</span>
        {#if unreadCount > 0}
            <span class="unread-count">{$invitations.length}</span>
        {/if}
        {#if settingsSrc}
            <a class="settings" href={settingsSrc}>
                <Icon src={Cog} />
            </a>
        {/if}
    </a>
</div>

<style lang="sass">
    .outer
        display: flex
        flex-direction: column
        
    .entry
        display: flex
        align-items: center
        gap: 0.4em

        padding: var(--vertical-padding) var(--horizontal-padding)
        margin-top: 0.2em

        border-radius: var(--radius)
        color: var(--on-background)
        text-decoration: none
        font-weight: 425
        cursor: pointer
        -webkit-tap-highlight-color: transparent

        &:hover, &.selected
            background-color: var(--background-hover)

            .settings
                display: block

        .icon
            width: 1.5em
            height: 1.5em
            color: var(--sidebar-icon-color)

        .unread-count
            $size: 1.3em
            margin-left: auto
            width: $size
            height: $size
            line-height: $size
            text-align: center

            background-color: crimson
            font-size: 0.7em
            font-weight: 550
            color: white
            border-radius: 100%

    :global(.drag-active-top::before, .drag-active-bottom::after)
        content: ''
        width: 100%
        border-bottom: 2px solid var(--primary)
        margin: 0.4em 0

    .settings
        display: none
        margin-left: auto
        width: 1.5em
        height: 1.5em
        color: var(--on-background-inactive)
        text-decoration: none

        &:hover
            color: var(--on-background)
</style>