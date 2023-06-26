<script lang="ts">
    import type {ActionData} from "../../../.svelte-kit/types/src/routes/login/$types";

    export let form : ActionData;
</script>

<form method="POST">
    {#if form?.incorrect}<p class="error">Invalid credentials</p>{/if}
    <input type="text" name="username" placeholder="Username..." value={form?.username ?? ""} />
    <input type="password" name="password" placeholder="Password..." />
    <input type="submit" value="Login" />
</form>
