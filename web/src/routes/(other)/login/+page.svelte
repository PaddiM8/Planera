<script lang="ts">
    import type {ActionData} from "../../../../.svelte-kit/types/src/routes/(other)/login/$types";
    import Input from "$lib/components/Input.svelte";
    import Button from "$lib/components/Button.svelte";
    import Form from "$lib/components/Form.svelte";

    export let form : ActionData;
</script>

<div id="content">
    <h1>Login</h1>

    <Form errors={form?.errors}>
        <Input name="username" placeholder="Username..." value={form?.username ?? ""} />
        <Input type="password" name="password" placeholder="Password..." />
        <Button value="Login" primary submit />
    </Form>
</div>

<style lang="sass">
    #content
        width: 25rem
        margin: auto 0
        padding-bottom: 20em
</style>