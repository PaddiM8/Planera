<script lang="ts">
import Form from "$lib/components/form/Form.svelte";
import Input from "$lib/components/form/Input.svelte";
import Button from "$lib/components/form/Button.svelte";
import ListBox from "$lib/components/form/ListBox.svelte";

export let data;
export let form;

async function handleAddParticipant(name: string): Promise<[string, boolean]> {
    const formData = new FormData();
    formData.append("username", name);
    const response = await fetch("?/addParticipant", {
        method: "POST",
        body: formData,
    });
    const result = await response.json();
    if (result.type === "success") {
        return [name, true];
    } else {
        return ["Failed to add participant.", false];
    }
}

async function handleRemoveParticipant(name: string): Promise<[string, boolean]> {
    const formData = new FormData();
    formData.append("username", name);
    const response = await fetch("?/removeParticipant", {
        method: "POST",
        body: formData,
    });
    const result = await response.json();
    if (result.type === "success") {
        return ["", true];
    } else {
        return ["Failed to remove participant.", false];
    }
}
</script>

<h1>Project Settings</h1>

<h2>About</h2>
<section class="about">
    <Form action="?/update" errors={form?.errors} reset={false}>
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
</section>

<hr>

<h2>Participants</h2>
<section class="participants">
    <ListBox items={data.project.participants.map(x => x.userName)}
             canAdd
             canRemove
             placeholder="Invite someone..."
             emptyText="No participants."
             addButtonText="Invite"
             handleAdd={handleAddParticipant}
             handleRemove={handleRemoveParticipant} />
</section>

<style lang="sass">
    section
        max-width: 35em

    .about
        .buttons
            display: flex
            gap: 0.4em
            margin-left: auto

    .participants
        height: 11.5em
</style>