<script lang="ts">
    import {
        Composer,
        ContentEditable,
        RichTextPlugin,
        ListPlugin,
        CheckListPlugin,
        HorizontalRulePlugin,
        ImagePlugin,
        HeadingNode,
        QuoteNode,
        ListNode,
        ListItemNode,
        HorizontalRuleNode,
        ImageNode,
        PlaceHolder,
        AutoLinkPlugin,
        AutoLinkNode,
        LinkPlugin,
        LinkNode,
        validateUrl,
        CodeNode,
        CodeHighlightNode,
        CaptionEditorHistoryPlugin,
        Toolbar,
        UndoButton,
        RedoButton,
        Divider,
        BlockFormatDropDown,
        ParagraphDropDownItem,
        HeadingDropDownItem,
        BulletDropDrownItem,
        NumberDropDrownItem,
        CheckDropDrownItem,
        QuoteDropDrownItem,
        CodeDropDrownItem,
        CodeLanguageDropDown,
        BoldButton,
        ItalicButton,
        UnderlineButton,
        StrikethroughButton,
        InsertLink,
        FormatCodeButton,
        InsertDropDown,
        InsertHRDropDownItem,
        DropDownAlign,
        HistoryPlugin,
        $getRoot as getRoot, FloatingLinkEditorPlugin, CodeHighlightPlugin, CodeActionMenuPlugin, CAN_USE_DOM,
    } from "svelte-lexical";

    import "./editor.css";
    import TaskEditorTheme from "$lib/components/editor/ticketEditorTheme";
    import {createEventDispatcher, onMount} from "svelte";
    import {
        $generateHtmlFromNodes as generateHtmlFromNodes,
        $generateNodesFromDOM as generateNodesFromDOM,
    } from '@lexical/html';
    import {browser} from "$app/environment";

    export let placeholder: string = "";

    const initialConfig = {
        namespace: "TaskEditor",
        theme: TaskEditorTheme,
        nodes: [
            HeadingNode,
            ListNode,
            ListItemNode,
            QuoteNode,
            HorizontalRuleNode,
            ImageNode,
            AutoLinkNode,
            LinkNode,
            CodeNode,
            CodeHighlightNode,
        ],
        onError: (error: Error) => {
            throw error;
        },
    };

    let editor;
    let editorShellElement: HTMLElement;
    let editorElement: HTMLElement;
    let composer: Composer;
    let isSmallWidthViewport = false;
    const dispatcher = createEventDispatcher();

    function updateViewPortWidth() {
        const isNextSmallWidthViewport = CAN_USE_DOM && window.matchMedia("(max-width: 1025px)").matches;

        if (isNextSmallWidthViewport !== isSmallWidthViewport) {
            isSmallWidthViewport = isNextSmallWidthViewport;
        }
    }

    onMount(() => {
        editor = composer.getEditor();
        for (const toolbarButton of editorShellElement.querySelectorAll(".toolbar button")) {
            toolbarButton.setAttribute("tabIndex", "-1");
        }

        const config = { attributes: false, childList: true, subtree: true };
        const observer = new MutationObserver(() => {
            dispatcher("input");
        });
        observer.observe(editorShellElement.querySelector("[contenteditable]"), config);

        // Keep track of viewport size
        window.addEventListener("resize", updateViewPortWidth);

        return () => {
            window.removeEventListener("resize", updateViewPortWidth);
        };
    });

    export function getHtml(): Promise<string> {
        return new Promise((resolve, _) => {
            editor.update(() => {
                resolve(generateHtmlFromNodes(editor));
            });
        });
    }

    export function setHtml(html: string) {
        const parser = new DOMParser();
        const dom = parser.parseFromString(html, "text/html");
        editor.update(() => {
            const nodes = generateNodesFromDOM(editor, dom);
            const root = getRoot();
            root.clear();
            for (const node of nodes) {
                root.append(node);
            }
        });
    }

    export function reset() {
        editor.update(() => {
            getRoot().clear();
        });
    }
</script>

<Composer {initialConfig} bind:this={composer}>
    <div class="editor-shell" bind:this={editorShellElement}>
        <Toolbar let:editor let:activeEditor let:blockType>
            <UndoButton />
            <RedoButton />
            <Divider />
            {#if activeEditor === editor}
                <BlockFormatDropDown>
                    <ParagraphDropDownItem />
                    <HeadingDropDownItem headingSize="h1" />
                    <HeadingDropDownItem headingSize="h2" />
                    <HeadingDropDownItem headingSize="h3" />
                    <BulletDropDrownItem />
                    <NumberDropDrownItem />
                    <CheckDropDrownItem />
                    <QuoteDropDrownItem />
                    <CodeDropDrownItem />
                </BlockFormatDropDown>
                <Divider />
            {/if}
            {#if blockType === "code"}
                <CodeLanguageDropDown />
            {:else}
                <BoldButton />
                <ItalicButton />
                <UnderlineButton />
                <StrikethroughButton />
                <InsertLink />
                <FormatCodeButton />
                <Divider />
                <InsertDropDown>
                    <InsertHRDropDownItem />
                </InsertDropDown>
                <Divider />
            {/if}
            <DropDownAlign />
            <Divider />
        </Toolbar>
        <div class="editor-container tree-view">
            <div class="editor-scroller">
                <div class="editor" bind:this={editorElement}>
                    <ContentEditable />
                    <PlaceHolder>{placeholder}</PlaceHolder>
                </div>
            </div>

            <AutoLinkPlugin />
            <HistoryPlugin />
            <RichTextPlugin />
            <ListPlugin />
            <CheckListPlugin />
            <HorizontalRulePlugin />
            <ImagePlugin>
                <CaptionEditorHistoryPlugin />
            </ImagePlugin>
            <LinkPlugin {validateUrl} />
            {#if !isSmallWidthViewport && browser}
                <FloatingLinkEditorPlugin anchorElem={editorElement} />
                <CodeHighlightPlugin />
                <CodeActionMenuPlugin anchorElem={editorElement} />
            {/if}
        </div>
    </div>
</Composer>

<style lang="sass">
    .editor-shell
        border: var(--border)
        border-radius: var(--radius)
        overflow: hidden
</style>