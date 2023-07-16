<script lang="ts">
    export let value: string;
    export let forId: string | undefined = undefined;
</script>

<label class="label" {forId}>{value}</label>

<style lang="sass">
    label
        margin-bottom: -0.4em
        font-size: 1em
        font-weight: 450
</style>