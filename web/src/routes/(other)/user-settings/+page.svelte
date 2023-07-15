<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {AccountDto} from "../../../gen/planeraClient.js";
    import {toast} from "$lib/toast";

    export let form;
    export let data: {
        account: AccountDto,
    };

    function afterSubmitUpdate(success: boolean) {
        if (success) {
            toast.info("Updated account successfully.");
        }
    }
</script>

<h1>Account Settings</h1>

<section class="update">
    <Form action="?/update"
          errors={form?.errors}
          afterSubmit={afterSubmitUpdate}
          reset={false}>
        <Input type="text"
               value={data.account.username}
               label="Username"
               name="username"
               placeholder="Username..." />
        <Input type="email"
               value={data.account.email}
               label="Email"
               name="email"
               placeholder="Email..." />
        <Button value="Update" primary submit />
    </Form>
</section>

<style lang="sass">
    section
        max-width: 35em
</style>