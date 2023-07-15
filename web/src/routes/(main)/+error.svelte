<script>
    import { page } from '$app/stores';
</script>

<h1>Error ({$page.status})</h1>
<h2>{$page.error.message}</h2>

<style lang="sass">
    h2
        font-weight: 450
</style>