<script>
    import { page } from '$app/stores';
</script>

<section class="error-page">
    <h1>Error ({$page.status})</h1>
    <h2>{$page.error.message}</h2>
</section>

<style lang="sass">
    h2
        font-weight: 450

    .error-page
        margin: 0 auto
        padding-top: 2em
</style>