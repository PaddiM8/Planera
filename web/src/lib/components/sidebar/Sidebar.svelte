<script lang="ts">
	import { onMount } from "svelte";
	import { beforeNavigate } from "$app/navigation";
	interface Props {
		children?: import('svelte').Snippet;
	}

	let { children }: Props = $props();

	const draggerWidth = 10;
	let sidebarElement: HTMLElement = $state()!;
	let dragging = false;

	onMount(() => {
		let startTouchX = 0;
		let startTouchY = 0;
		document.ontouchmove = (e) => {
			if (!dragging) {
				return;
			}

			if (shouldIgnoreTouchEvent(e)) {
				return;
			}

			let offsetX = Math.min(e.touches[0].clientX - startTouchX, sidebarElement.clientWidth);
			const offsetY = Math.abs(e.touches[0].clientY - startTouchY);
			const isOpen = sidebarElement.classList.contains("open")
			if (!isOpen) {
				if (offsetX < 50 || offsetY > 150 || Math.abs(offsetY) > Math.abs(offsetX)) {
					return;
				}
			}
		
			if (isOpen) {
				offsetX = Math.min(0, offsetX);
				sidebarElement.style.transform = `translateX(${offsetX}px)`;
			} else {
				sidebarElement.style.transform = `translateX(calc(-100% + ${offsetX}px))`;
			}
		};
		document.ontouchend = endDrag;
		document.ontouchstart = e => {
			if (e.touches.length !== 1) {
				return;
			}
			
			if (shouldIgnoreTouchEvent(e)) {
				return;
			}
			
			startDrag();
			startTouchX = e.touches[0].clientX;
			startTouchY = e.touches[0].clientY;
		};
	});

	beforeNavigate(close);

	function startDrag() {
		dragging = true;
		document.body.style.userSelect = "none";
		sidebarElement.style.transition = "none";
	}

	function endDrag(e: TouchEvent) {
		if (!dragging) {
			return;
		}
		
		if (shouldIgnoreTouchEvent(e)) {
			return;
		}

		dragging = false;
		sidebarElement.style.transition = "";
		document.body.style.userSelect = "";

		const rect = sidebarElement.getBoundingClientRect();
		const isOpen = sidebarElement.classList.contains("open");
		if (!isOpen && rect.left + rect.width < rect.width / 4) {
			close();
		} else if(isOpen && rect.left < -rect.width / 5) {
			close();
		} else {
			sidebarElement.classList.add("open");
		}

		sidebarElement.style.transform = "";
	}
	
	function shouldIgnoreTouchEvent(event: TouchEvent) {
		for (const touched of event.touches) {
			let target: any = touched.target;
			while (target && !shouldIgnoreTouchEventForTarget(target)) {
				target = target.parentElement;
			}

			if (target) {
				return true;
			}
		}
		
		return false;
	}
	
	function shouldIgnoreTouchEventForTarget(target: HTMLElement) {
		if (target.classList.contains("sidebar-button")) {
			return true;
		}
		
		return target.scrollLeft > 0;
	}

	function close() {
		sidebarElement?.classList.add("closing");
		setTimeout(() => {
			sidebarElement?.classList.remove("open");
			sidebarElement?.classList.remove("closing");
		}, 350);
	}
</script>

<aside id="sidebar" bind:this={sidebarElement}>
	{@render children?.()}
</aside>
<!-- svelte-ignore a11y_click_events_have_key_events -->
<div class="outside" onclick={close} role="button" tabindex="-1"></div>
<div class="dragger" style="width: {draggerWidth}px" ontouchstart={startDrag} role="button" tabindex="-1"></div>

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
            width: 60vw
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
