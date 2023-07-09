<script lang="ts">
    export let value: string;
</script>

<span class="label">{ value }</span>

<style lang="sass">
    .label
        margin-top: 0.2em
        margin-left: 0.2em
        color: var(--text-gray)
        text-transform: uppercase
        font-weight: 600
        font-size: 0.8em
</style>