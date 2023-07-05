<script lang="ts">
    import type {Project} from "../../../../../gen/planeraClient";
    import Form from "$lib/components/Form.svelte";
    import Input from "$lib/components/Input.svelte";
    import Button from "$lib/components/Button.svelte";
    import Editor from "$lib/components/editor/Editor.svelte";

    export let data: {
        project: Project | undefined,
        error: boolean,
    };
</script>

<section class="description">
    <h1>{ data.project.name }</h1>
    <h3>A short description of the project.</h3>
</section>

<section class="new-ticket">
    <h2>New Ticket</h2>
    <Form>
        <Input type="text" name="title" placeholder="Title..." />
        <Editor />
        <Button value="Create" primary submit />
    </Form>
</section>

<section class="tickets">
    <h2>Tickets</h2>
</section>

<style lang="sass">
    section
        border-bottom: var(--border)
        margin-bottom: 1em
        padding-bottom: 1em
</style>