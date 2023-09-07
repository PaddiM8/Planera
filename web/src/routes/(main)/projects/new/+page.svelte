<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import AvatarPicker from "$lib/components/form/AvatarPicker.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";
    import Button from "$lib/components/form/Button.svelte";

    export let form;
    export let data;

    let name: string = "";
    let slug: string;

    $: {
        slug = name
            .split("")
            .map(x => x === " " ? "-" : x.toLowerCase())
            .filter(x => x.match(/[a-z0-9\- ]/))
            .join("");
    }
</script>

<h1>New Project</h1>
<section class="form">
    <Form problem={form?.problem} promptWhenModified>
        <input type="hidden" name="username" value={data.user.username} />
        <div class="avatar-area">
            <FormLabel value="Project Icon" />
            <AvatarPicker name="icon"
                          bind:entityName={name}
                          type="project" />
        </div>
        <div class="name-area">
            <Input type="text"
                   name="name"
                   placeholder="Project Name..."
                   label="Name"
                   bind:value={name} />
            <Input type="text"
                   name="slug"
                   placeholder="project-url-name..."
                   label="URL"
                   bind:value={slug} />
        </div>
        <Input type="text"
               name="description"
               placeholder="Description..."
               label="Description" />
        <Button value="Create" primary submit />
    </Form>
</section>

<style lang="sass">
    .form
        max-width: 35em

    .name-area
        display: flex
        gap: var(--spacing)

    :global(.name-area .wrapper)
        display: flex
        flex-direction: column

    :global(.name-area input)
        margin-top: auto
</style>