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
        FontFamilyDropDown,
        FontSizeDropDown,
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
    } from "svelte-lexical";

    import "./editor.css";
    import TaskEditorTheme from "$lib/components/editor/ticketEditorTheme";
    import {onMount} from "svelte";
    import {$generateHtmlFromNodes as generateHtmlFromNodes} from '@lexical/html';

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
    let composer: Composer;

    onMount(() => {
        editor = composer.getEditor();
    });

    export function getHtml(): Promise<string> {
        return new Promise((resolve, _) => {
            editor.update(() => {
                resolve(generateHtmlFromNodes(editor));
            });
        });
    }

    export function reset() {
        editor.update(() => {
            getRoot().clear();
        });
    }
</script>

<Composer {initialConfig} bind:this={composer}>
    <div class="editor-shell">
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
                <FontFamilyDropDown />
                <FontSizeDropDown />
                <Divider />
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
        </Toolbar>
        <div class="editor-container tree-view">
            <div class="editor-scroller">
                <div class="editor">
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
        </div>
    </div>
</Composer>

<style lang="sass">
    .editor-shell
        border: var(--border)
        border-radius: var(--radius)
        overflow: hidden
</style>