<script lang="ts">
	import { onMount } from 'svelte';
	import { beforeNavigate } from '$app/navigation';

	const draggerWidth = 10;
	let sidebarElement: HTMLElement;
	let dragging = false;

	onMount(() => {
		document.ontouchmove = (e) => {
			if (e.touches.length !== 1) {
				return;
			}

			const touchX = e.touches[0].clientX;
			if (!dragging || touchX <= draggerWidth) {
				return;
			}

			const offset = Math.min(touchX, sidebarElement.clientWidth);
			sidebarElement.style.transform = `translateX(calc(-100% + ${offset}px))`;
			sidebarElement.classList.add('open');
		};
		document.ontouchend = endDrag;
		document.ontouchstart = () => {
			if (sidebarElement.classList.contains('open')) {
				startDrag();
			}
		};
	});

	beforeNavigate(close);

	function startDrag() {
		dragging = true;
		document.body.style.userSelect = 'none';
		sidebarElement.style.transition = 'none';
	}

	function endDrag() {
		if (!dragging) {
			return;
		}

		dragging = false;
		sidebarElement.style.transition = '';
		document.body.style.userSelect = '';

		const rect = sidebarElement.getBoundingClientRect();
		if (rect.left < -rect.width / 2) {
			close();
		}

		sidebarElement.style.transform = '';
	}

	function close() {
		sidebarElement.classList.add('closing');
		setTimeout(() => {
			sidebarElement.classList.remove('open');
			sidebarElement.classList.remove('closing');
		}, 350);
	}
</script>

<aside id="sidebar" bind:this={sidebarElement}>
	<slot />
</aside>
<!-- svelte-ignore a11y-click-events-have-key-events -->
<div class="outside" on:click={close} />
<div class="dragger" style="width: {draggerWidth}px" on:touchstart={startDrag} />

<style lang="sass">
    @use "../../../values"

    aside
        display: flex
        flex-direction: column
        padding: 0.4em
        border-right: var(--border)
        background-color: var(--background)
        user-select: none
        overflow-y: auto

    .outside
        visibility: collapse
        position: absolute
        top: 0
        left: 0
        width: 100vw
        height: 100vh
        background-color: black
        opacity: 0.0
        transition: 350ms ease opacity
        z-index: 999998

    .dragger
        position: absolute
        display: none
        top: 3.3em
        left: 0
        height: calc(100vh - 3.3em)

    @media screen and (max-width: values.$max-width-for-hidden-sidebar)
        aside
            position: absolute
            top: 0
            left: 0
            width: 60%
            max-width: 300px
            height: 100vh
            transform: translateX(-100%)
            transition: 350ms ease transform
            box-sizing: border-box
            z-index: 999999

        .dragger
            display: block

        :global(aside.open)
            transform: translateX(0)

        :global(aside.closing)
            transform: translateX(-100%)

        :global(aside.closing + .outside)
            opacity: 0.0 !important

        :global(aside.open + .outside)
            visibility: visible
            opacity: 0.6
</style>
