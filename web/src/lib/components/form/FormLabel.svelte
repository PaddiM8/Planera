<script lang="ts">
    interface Props {
        value: string;
        forId?: string | undefined;
    }

    let { value, forId = undefined }: Props = $props();
</script>

<!-- svelte-ignore a11y_label_has_associated_control -->
<label class="label" for={forId}>{value}</label>

<style lang="sass">
    label
        display: block
        font-size: 1em
        font-weight: 450
        margin-bottom: 0.4em
</style>