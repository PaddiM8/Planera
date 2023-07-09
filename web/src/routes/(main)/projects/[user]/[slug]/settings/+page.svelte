<script lang="ts">
import Form from "$lib/components/form/Form.svelte";
import Input from "$lib/components/form/Input.svelte";
import Button from "$lib/components/form/Button.svelte";
import BlockInput from "$lib/components/form/BlockInput.svelte";

export let data;
export let form;
</script>

<h1>Project Settings</h1>

<Form errors={form?.errors} reset={false}>
    <Input type="text"
           value={data.project.name}
           label="Name"
           name="name"
           placeholder="Project name..." />
    <Input type="text"
           value={data.project.description}
           label="Description"
           name="description"
           placeholder="Project description..." />
    <div class="buttons">
        <a href="/projects/{data.project.author.userName}/{data.project.slug}">
            <Button value="Cancel" />
        </a>
        <Button value="Update" primary submit />
    </div>
</Form>

<style lang="sass">
    .buttons
        display: flex
        gap: 0.4em
        margin-left: auto
</style>