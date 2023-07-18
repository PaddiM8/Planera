<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {AccountDto} from "../../../gen/planeraClient.js";
    import {toast} from "$lib/toast";
    import FormLabel from "$lib/components/form/FormLabel.svelte";
    import {getAvatarUrl} from "$lib/clients";
    import {invalidateAll} from "$app/navigation";
    import AvatarPicker from "$lib/components/form/AvatarPicker.svelte";

    export let form;
    export let data: {
        account: AccountDto,
    };

    function afterSubmitUpdate(success: boolean) {
        if (success) {
            toast.info("Updated account successfully.");

            // Reload to make the changes update everywhere
            invalidateAll();
        }
    }

    function afterSubmitChangePassword(success: boolean) {
        if (success) {
            toast.info("Updated password successfully.");
        }
    }

</script>

<h1>Account Settings</h1>

<h2>Edit Account</h2>
<section class="update">
    <Form action="?/update"
          problem={form?.problem}
          afterSubmit={afterSubmitUpdate}
          reset={false}>
        <div class="avatar-area">
            <FormLabel value="Profile Picture" />
            <AvatarPicker name="avatar"
                          entityName={data.account.username}
                          src={getAvatarUrl(data.account.avatarPath, "big")}
                          type="user" />
        </div>
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

<h2>Change Password</h2>
<section class="password-change">
    <Form action="?/changePassword"
          problem={form?.changePassword?.errors}
          afterSubmit={afterSubmitChangePassword}>
        <Input type="password"
               label="Current Password"
               name="currentPassword"
               placeholder="Current Password..." />
        <Input type="password"
               label="New Password"
               name="newPassword"
               placeholder="New Password..." />
        <Input type="password"
               label="Confirm Password"
               name="confirmedPassword"
               placeholder="Confirm Password..." />

        <Button value="Update" primary submit />
    </Form>
</section>

<style lang="sass">
    section
        max-width: 35em
</style>