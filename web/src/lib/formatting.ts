import * as xss from "xss";

function isToday(date: Date) {
    const now = new Date();

    return date.getFullYear() == now.getFullYear()
        && date.getMonth() == now.getMonth()
        && date.getDate() == now.getDate();
}

function zeroPad(value: number) {
    const string = value.toString();

    return string.length === 1
        ? `0${string}`
        : string;
}

export function formatDate(date: Date) {
    return isToday(date)
        ? `${zeroPad(date.getHours())}:${zeroPad(date.getMinutes())}`
        : `${zeroPad(date.getDate())}/${zeroPad(date.getMonth() + 1)}`;
}

const sanitizerOptions = {
    whiteList: {
        h1: ["dir"],
        h2: ["dir"],
        h3: ["dir"],
        h4: ["dir"],
        p: ["dir"],
        i: ["dir"],
        b: ["dir"],
        u: ["dir"],
        s: ["dir"],
        strong: ["dir"],
        a: ["href", "title", "target", "dir"],
        img: ["src", "alt", "width", "height", "dir"],
        hr: ["dir"],
        br: ["dir"],
        ul: ["dir"],
        ol: ["dir"],
        li: ["role", "value", "dir"],
        blockquote: ["dir"],
        code: ["spellcheck", "dir"],
        span: ["dir"]
    },
    onIgnoreTagAttr: (tag: string, name: string, value: string, isWhiteAttr: boolean) => {
        if (name === "class" && value.startsWith("TicketEditorTheme__")) {
            return `class="${value.split(" ")[0]}"`;
        }

        if (name.startsWith("data-")) {
            return `${name}="${value}"`;
        }

        if (name === "style") {
            const allowedStyles = [
                "text-align",
                "padding-inline-start",
                "user-select",
            ];
            if (allowedStyles.some(value.startsWith)) {
                return `style="${value.split(";")[0]}"`;
            }
        }
    }
};
const sanitizer = new xss.FilterXSS(sanitizerOptions);

export function sanitizeHtml(html: string) {
    return sanitizer.process(html);
}