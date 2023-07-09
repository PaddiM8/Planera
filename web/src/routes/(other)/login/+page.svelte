<script lang="ts">
    import type {ActionData} from "../../../../.svelte-kit/types/src/routes/(other)/login/$types";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import Form from "$lib/components/form/Form.svelte";

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
        width: 100%
        max-width: 25rem
        margin-left: 50%
        margin-top: 50%
        transform: translate(-50%, -50%)
        padding-bottom: 20em
</style>