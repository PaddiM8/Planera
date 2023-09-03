<script lang="ts">
    import UserIcon from "$lib/components/UserIcon.svelte";
    import FileInput from "$lib/components/form/FileInput.svelte";
    import Button from "$lib/components/form/Button.svelte";

    export let name: string;
    export let entityName: string;
    export let src: string | undefined = undefined;
    export let type: "user" | "project";

    let avatarInput: HTMLInputElement;

    async function handleAvatarFile(e) {
        const target = e.detail.target as HTMLInputElement;
        if (target.files.length == 0) {
            return;
        }

        const file = target.files![0];
        const image = new Image();
        image.onload = () => {
            const aspectRatio = image.width / image.height;
            const maxWidth = 512;
            const maxHeight = 512;

            let newWidth = image.width;
            let newHeight = image.height;

            if (newWidth > maxWidth) {
                newWidth = maxWidth;
                newHeight = newWidth / aspectRatio;
            }

            if (newHeight > maxHeight) {
                newHeight = maxHeight;
                newWidth = newHeight * aspectRatio;
            }

            const canvas = document.createElement("canvas");
            canvas.width = newWidth;
            canvas.height = newHeight;

            const context = canvas.getContext("2d");
            context.drawImage(image, 0, 0, newWidth, newHeight);

            src = canvas.toDataURL(file.type);
        };

        image.src = URL.createObjectURL(file);
    }

    function handleClear() {
        src = undefined;
        avatarInput.value = "";
    }
</script>

<div class="avatar-area">
    <div class="icon">
        <UserIcon name={entityName}
                  image={src}
                  type={type} />
    </div>
    <FileInput accept={["image/png", "image/jpeg"]}
               bind:this={avatarInput}
               on:change={handleAvatarFile} />
    <input type="hidden"
           {name}
           bind:value={src} />
    <Button value="Clear" on:click={handleClear} />
</div>

<style lang="sass">
    .avatar-area
        display: flex
        align-items: center
        gap: 0.4em
        width: fit-content
        padding: var(--vertical-padding) var(--horizontal-padding)
        border: var(--border)
        border-radius: var(--radius)
        background-color: var(--component-background)

        .icon
            width: 1.2em
            font-size: 2.75em

    :global(.avatar-area button)
        align-self: auto
</style>