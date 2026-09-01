<script lang="ts">
    import FileInput from "$lib/components/form/FileInput.svelte";
    import {Icon, Photo} from "svelte-hero-icons";

    interface Props {
        name?: string;
    }

    let {
        name,
    }: Props = $props();
    let fileInput: FileInput = $state()!;
    let imageUrl: string | undefined = $state();
    
    export async function getBase64Url(): Promise<string | null> {
        return await fileInput.getBase64Url();
    }
    
    export function clear() {
        imageUrl = undefined;
    }

    async function renderPreview(event: Event) {
        const target = event.target as HTMLInputElement;
        if (target.files?.length == 0) {
            return;
        }

        const file = target.files![0];
        imageUrl = URL.createObjectURL(file);
    }
</script>

<div class="input-area">
    <span class="preview">
        <!-- TODO: Hero icon called "image" -->
        {#if imageUrl}
            <img src={imageUrl} width="32px" height="auto" />
        {:else}
            <span class="icon">
                <Icon src={Photo} size="32" />
            </span>
        {/if}
    </span>
    <FileInput accept={["image/png", "image/jpeg"]}
               bind:this={fileInput}
               onchange={renderPreview} />
    {#if name}
        <input type="hidden"
               {name}
               bind:value={imageUrl} />
    {/if}
</div>

<style lang="sass">
    .input-area
        display: flex
        align-items: center
        gap: 0.4em
        width: fit-content
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

    :global(.input-area button)
        align-self: auto
        
    .preview
        width: 2em
        height: 1.75em
        overflow: hidden
        
    :global(.input-area > .preview > .icon > *)
        // The SVG is a bit unaligned unfortunately
        margin-top: -2.5px
</style>
