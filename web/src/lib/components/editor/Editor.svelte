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
        DropDownAlign, HistoryPlugin, KeywordNode,
    } from "svelte-lexical";

    import "./editor.css";
    import TaskEditorTheme from "$lib/Editor/taskEditorTheme";
    import {onMount} from "svelte";

    let editorShellElement: HTMLElement;

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
            KeywordNode,
            AutoLinkNode,
            LinkNode,
            CodeNode,
            CodeHighlightNode,
        ],
        onError: (error: Error) => {
            throw error;
        },
    };

    onMount(() => {
        // Slight hack until the next version of svelte-lexical is released.
        // Without this, clicking dropdowns would cause forms to submit, which
        // is fixed but not released.
        // TODO: Remove this when possible.
        for (const button of editorShellElement.querySelectorAll("button")) {
            button.setAttribute("type", "button");
        }
    })
</script>

<Composer {initialConfig}>
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
                    <PlaceHolder>Describe the task...</PlaceHolder>
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