<script lang="ts">
    import type {Project} from "../../../../../gen/planeraClient";
    import Form from "$lib/Form.svelte";
    import Input from "$lib/Input.svelte";
    import Button from "$lib/Button.svelte";
    import Editor from "$lib/Editor/Editor.svelte";
    import {RichTextComposer} from "svelte-lexical";

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