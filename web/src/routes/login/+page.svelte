<script lang="ts">
    import { enhance } from "$app/forms";
    import type {ActionData} from "../../../.svelte-kit/types/src/routes/login/$types";
    import FormErrors from "$lib/FormErrors.svelte";

    export let form : ActionData;
</script>

<form method="POST" use:enhance>
    <FormErrors errors={form?.errors} />
    <input type="text" name="username" placeholder="Username..." value={form?.username ?? ""} />
    <input type="password" name="password" placeholder="Password..." />
    <input type="submit" value="Login" />
</form>
