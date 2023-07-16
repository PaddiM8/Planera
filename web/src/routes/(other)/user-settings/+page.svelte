<script lang="ts">
    import Form from "$lib/components/form/Form.svelte";
    import Input from "$lib/components/form/Input.svelte";
    import Button from "$lib/components/form/Button.svelte";
    import {AccountDto} from "../../../gen/planeraClient.js";
    import {toast} from "$lib/toast";
    import UserIcon from "$lib/components/UserIcon.svelte";
    import FileInput from "$lib/components/form/FileInput.svelte";
    import FormLabel from "$lib/components/form/FormLabel.svelte";
    import {onMount} from "svelte";
    import {getAvatarUrl} from "$lib/clients";
    import {invalidateAll} from "$app/navigation";

    export let form;
    export let data: {
        account: AccountDto,
    };

    let avatarInput: HTMLInputElement;
    let avatarUrl: string | undefined = data?.account.avatarPath
        ? getAvatarUrl(data.account.avatarPath, "big")
        : undefined;

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

    async function handleAvatarFile(e) {
        const target = e.detail.target as HTMLInputElement;
        if (target.files.length == 0) {
            return;
        }

        const reader = new FileReader();
        reader.onload = (event) => {
            avatarUrl = event.target.result as string;
        }

        reader.readAsDataURL(target.files![0]);
    }

    function handleClear() {
        avatarUrl = undefined;
        avatarInput.value = "";
    }
</script>

<h1>Account Settings</h1>

<h2>Edit Account</h2>
<section class="update">
    <Form action="?/update"
          errors={form?.update?.errors}
          afterSubmit={afterSubmitUpdate}
          reset={false}>
        <FormLabel value="Profile Picture" />
        <div class="avatar-area">
            <div class="icon">
                <UserIcon name={data.account.username}
                          image={avatarUrl}
                          type="user" />
            </div>
            <FileInput accept={["image/png", "image/jpeg"]}
                       bind:this={avatarInput}
                       on:change={handleAvatarFile} />
            <input type="hidden"
                   name="avatar"
                   bind:value={avatarUrl} />
            <Button value="Clear" on:click={handleClear} />
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
          errors={form?.changePassword?.errors}
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

    .avatar-area
        display: flex
        align-items: center
        gap: 0.4em

        .icon
            width: 1.2em
            font-size: 2.75em

    :global(.avatar-area button)
        align-self: auto
</style>