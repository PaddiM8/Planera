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
        $getRoot as getRoot,
        FloatingLinkEditorPlugin,
        CodeHighlightPlugin,
        CodeActionMenuPlugin,
    } from "@paddim8/svelte-lexical";
    import "./editor.css";
    import TaskEditorTheme from "$lib/components/editor/ticketEditorTheme";
    import {createEventDispatcher, onMount} from "svelte";
    import {browser} from "$app/environment";
    import * as lexicalHtml from "@lexical/html";
    const generateHtmlFromNodes = lexicalHtml.$generateHtmlFromNodes;
    const generateNodesFromDOM = lexicalHtml.$generateNodesFromDOM;
    import * as lexical from "lexical";
    const createParagraphNode = lexical.$createParagraphNode;
    const getSelection = lexical.$getSelection;

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

    let editor: any;
    let editorShellElement: HTMLElement;
    let editorElement: HTMLElement;
    let composer: Composer;
    let hasFocus = false;
    const dispatcher = createEventDispatcher();

    onMount(() => {
        editor = composer.getEditor();
        for (const toolbarButton of editorShellElement.querySelectorAll(".toolbar button")) {
            toolbarButton.setAttribute("tabIndex", "-1");
        }

        const config = { attributes: false, childList: true, subtree: true };
        const observer = new MutationObserver(() => {
            dispatcher("input");

            // Using regular JavaScript events for proper bubbling
            const event = new CustomEvent("input", {
                bubbles: true
            })
            editorShellElement.dispatchEvent(event);
        });
        observer.observe(editorShellElement.querySelector("[contenteditable]")!, config);

        const contentEditable = editorElement.querySelector("[contenteditable]") as HTMLElement;
        contentEditable.onfocus = () => hasFocus = true;
        contentEditable.onblur = () => hasFocus = false;
    });

    export function getHtml(): Promise<string> {
        return new Promise((resolve, _) => {
            editor.update(() => {
                let html = generateHtmlFromNodes(editor);
                const emptyLineMarkups = [
                    '<p class="TicketEditorTheme__paragraph"><br></p>',
                    '<p class="TicketEditorTheme__paragraph" dir="ltr"><br></p>',
                    '<p class="TicketEditorTheme__paragraph" dir="rtl"><br></p>',
                ];
                let toTrim: string | undefined;
                while (toTrim = emptyLineMarkups.find(x => html.endsWith(x))) {
                    html = html.slice(0, -toTrim.length);
                }
                resolve(html);
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
                root.append(node as any);
            }
        });
    }

    export function reset() {
        editor.update(() => {
            getRoot().clear();
        });
        editor.update(() => {
            const root = getRoot();
            const selection = getSelection();
            const paragraph = createParagraphNode();
            root.clear();
            root.append(paragraph as any);

            if (selection !== null) {
                paragraph.select();
            }
        });
    }
</script>

<Composer {initialConfig} bind:this={composer}>
    <div class="editor-shell" class:has-focus={hasFocus} bind:this={editorShellElement}>
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
            {#if browser}
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
