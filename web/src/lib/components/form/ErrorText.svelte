<script lang="ts">
    interface Props {
        value: string;
    }

    let { value }: Props = $props();
</script>

<span class="error">{ value }</span>

<style lang="sass">
    .error
        color: var(--red)
        margin-top: 0.2em
</style>