<script lang="ts">
    import Select from "$lib/components/form/Select.svelte";
    import {AccountDto, AuthenticationInfo, InterfaceTheme, NotificationKinds} from "../../../../gen/planeraClient";
    import {getKeyFromValue} from "$lib/util";
    import {userHub} from "../store";
    import {theme} from "../../../store"
    import FormLabel from "$lib/components/form/FormLabel.svelte";
    import MultiButton from "$lib/components/form/MultiButton.svelte";
    import {checkNotificationsEnabled, subscribeToPushNotifications} from "$lib/notifications";
    import Form from "$lib/components/form/Form.svelte";
    import {toast} from "$lib/toast";
    import type {ProblemDetails} from "$lib/problemDetails";
    import {browser} from "$app/environment";
    import Button from "$lib/components/form/Button.svelte";

    interface Props {
        data: {
    account: AccountDto,
    authenticationInfo: AuthenticationInfo,
};
        form: {
    errors: { string: string[] } | undefined,
    problem: ProblemDetails,
};
    }

    let { data = $bindable(), form }: Props = $props();

const themeMap : { [key: string]: InterfaceTheme }= {
    "System": InterfaceTheme.System,
    "Light": InterfaceTheme.Light,
    "Dark": InterfaceTheme.Dark,
};
let themeName = $state(getKeyFromValue(themeMap, data.account.theme)!);
let enableNotifications = $state(checkNotificationsEnabled() && data.account.enabledNotificationKinds?.includes(NotificationKinds.Core)
    ? "true"
    : "false");
let notifyDeadlines = data.account.enabledNotificationKinds?.includes(NotificationKinds.DeadlineMyTicket)
    ? "true"
    : "false";

async function updateTheme() {
    data.account.theme = themeMap[themeName];
    $theme = data.account.theme;
    await $userHub?.invoke("setTheme", data.account.theme);
}

async function handleEnableNotifications() {
    if (!browser) {
        return;
    }

    if (enableNotifications == "true" && !checkNotificationsEnabled() && $userHub) {
        await subscribeToPushNotifications($userHub, data.authenticationInfo.vapidPublicKey!);
    }
}

function afterSubmitConfigureNotifications(success: boolean) {
    if (success) {
        toast.info("Updated notification settings successfully.");
    } else {
        toast.error("Failed to update notification settings.");
    }
}
</script>

<svelte:head>
    <title>General Settings - Planera</title>
</svelte:head>

<h1>General</h1>

<h2>Appearance</h2>
<section class="theme-selection">
    <FormLabel value="Theme" />
    <Select choices={["System", "Light", "Dark"]}
            bind:selectedValue={themeName}
            on:change={updateTheme} />
</section>

<hr>

<h2>Notifications</h2>
<section class="notifications">
    <Form action="?/configureNotifications"
          problem={form?.problem}
          afterSubmit={afterSubmitConfigureNotifications}
          reset={false}>
        <div>
            <FormLabel value="Enable notifications" />
            <MultiButton yesNo
                         name="enable-notifications"
                         bind:selectedValue={enableNotifications}
                         on:change={handleEnableNotifications} />
        </div>

        <div>
            <FormLabel value="Notify me about deadlines" />
            <MultiButton yesNo
                         name="notify-deadlines"
                         selectedValue={notifyDeadlines}
                         disabled={enableNotifications == "false"} />
        </div>
        <Button value="Save" primary submit />
    </Form>
</section>

<style lang="sass">
    section
        max-width: 35em
        margin-bottom: 1.75em

    .theme-selection
      width: 100%
      max-width: 15em
</style>