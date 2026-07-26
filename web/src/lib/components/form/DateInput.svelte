<script lang="ts">
    import FormLabel from "$lib/components/form/FormLabel.svelte";

    export let value: Date | undefined;
    export let name: string = "";
    export let label: string | undefined = undefined;
    export let time: boolean = false;

    let wrapperElement: HTMLElement;
    let dateString: string | undefined;
    
    $: dateString = toDateString(value);
    
    export function focus() {
        (wrapperElement.firstElementChild as HTMLInputElement).focus();
    }
    
    function toDateString(date: Date | undefined) {
        if (!date) {
            return undefined;
        }

        const newDate = new Date(date);
        const pad = (n: number) => String(n).padStart(2, "0");

        const year = newDate.getFullYear();
        const month = pad(newDate.getMonth() + 1);
        const day = pad(newDate.getDate());
        const hours = pad(newDate.getHours());
        const minutes = pad(newDate.getMinutes());

        if (time) {
            return `${year}-${month}-${day}T${hours}:${minutes}`;
        } else {
            return `${year}-${month}-${day}`;
        }
    }
    
    function handleDateChange(event: Event) {
        const target = event.target as HTMLInputElement;
        value = new Date(target.value);
    }
</script>

<div class="wrapper" bind:this={wrapperElement}>
    {#if label}
        <FormLabel forId="input-{name}" value={label} />
    {/if}

    {#if time}
        {#if value}
            <input type="datetime-local"
                   id="input-{name}"
                   bind:value={dateString}
                   on:input
                   {name} />
        {:else}
            <input type="date"
                   id="input-{name}"
                   on:input
                   on:change={handleDateChange}
                   {name} />
        {/if}
    {:else}
        <input type="date"
               id="input-{name}"
               bind:value={value}
               on:input
               {name} />
    {/if}
</div>

<style lang="sass">
    .wrapper
        width: 100%

    input
        display: block
        width: 100%
        font-size: 1rem
        padding: var(--vertical-padding) var(--horizontal-padding)
        border-radius: var(--radius)
        border: 0
        color: var(--on-background)
        background-color: var(--component-background)
        outline: var(--border)
        box-sizing: border-box
        color-scheme: var(--color-scheme)
        letter-spacing: -0.75px

        &:focus
            outline-width: 2px
            outline-color: var(--primary)
</style>
