import "../css/admin.scss";
import loader from '@monaco-editor/loader';
import type { editor } from "monaco-editor";

type Language = "html" | "json" | "css";
type EditorOptions = {
    lineNumbers?: boolean,
    readonly?: boolean,
    formatted?: boolean,
    folding?: boolean,
    adjustHeightToContent?: boolean,
}

const editors = new Map<HTMLElement, editor.IStandaloneCodeEditor>();
const runtimeOptions = new Map<HTMLElement, { height: string | undefined }>();

const getLineNumberOptions = (options: EditorOptions): editor.LineNumbersType => {
    if (options.lineNumbers) {
        return "on"
    }
    return "off";
}

export function CreateMonacoEditor(component: DotNet.DotNetObject, el: HTMLElement, value: string, language: Language, options: EditorOptions) {
    loader.init().then(async i => {
        console.log(options);
        if (language === "json" && options.formatted) {
            value = JSON.stringify(JSON.parse(value), undefined, 4);
        }
        if (editors.has(el)) {
            editors.get(el)?.dispose();
        }
        let lastHeight: string | undefined;

        const edit = i.editor.create(el, {
            value: value,
            language: language,
            theme: 'vs-dark',
            minimap: {
                enabled: false
            },
            lineNumbers: getLineNumberOptions(options),
            folding: options.folding,
            readOnly: options.readonly,
            automaticLayout: true,
            scrollBeyondLastLine: false,
        });
        runtimeOptions.set(el, {});
        editors.set(el, edit);
        edit.onKeyUp(async _ => {
            const newText = edit.getValue();
            await component.invokeMethodAsync('UpdateValue', newText);
        });

        const reportHeight = async () => {
            if (options.adjustHeightToContent && edit) {
                const lineCount = edit.getModel()?.getLineCount();
                const contentHeight = (lineCount ?? 1) * 19;
                console.log("reportHeight", contentHeight);
                const newHeight = `${contentHeight}px`;
                lastHeight = newHeight;
                runtimeOptions.set(el, {
                    height: lastHeight
                });
                await component.invokeMethodAsync('SetHeight', newHeight);
            }
        }

        edit.onDidChangeModel(async () => {
            console.log("onDidChangeModel");
            await reportHeight();
        });
        await reportHeight();
    });
}

export async function UpdateOptions(component: DotNet.DotNetObject, el: HTMLElement) {
    if(runtimeOptions.has(el)){
        const opt = runtimeOptions.get(el);
        if(opt?.height) {
            await component.invokeMethodAsync('SetHeight', opt.height);
        }
    }
}

export function DisposeMonacoEditor(el: HTMLElement) {
    const edit = editors.get(el);
    if (edit) {
        edit.dispose();
    }
}