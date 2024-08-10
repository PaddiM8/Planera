<script lang="ts">
import Select from "$lib/components/form/Select.svelte";
import {AccountDto, InterfaceTheme} from "../../../../gen/planeraClient";
import {getKeyFromValue} from "$lib/util";
import {userHub} from "../store";
import {theme} from "../../../store"
import FormLabel from "$lib/components/form/FormLabel.svelte";
export let data: {
    account: AccountDto,
};

const themeMap : { [key: string]: InterfaceTheme }= {
    "Light": InterfaceTheme.Light,
    "Dark": InterfaceTheme.Dark,
};
let themeName = getKeyFromValue(themeMap, data.account.theme)!;

async function updateTheme() {
    data.account.theme = themeMap[themeName];
    $theme = data.account.theme;
    await $userHub?.invoke("setTheme", data.account.theme);
}
</script>

<svelte:head>
    <title>General Settings - Planera</title>
</svelte:head>

<h1>General</h1>

<h2>Appearance</h2>
<div class="theme-selection">
    <FormLabel value="Theme" />
    <Select choices={["Light", "Dark"]}
            bind:selectedValue={themeName}
            on:change={updateTheme} />
</div>

<style lang="sass">
    .theme-selection
      width: 100%
      max-width: 15em
</style>