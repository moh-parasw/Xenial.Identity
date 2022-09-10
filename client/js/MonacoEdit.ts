import "../css/admin.scss";
import loader from '@monaco-editor/loader';
import type { editor } from "monaco-editor";

type language = "html" | "json" | "css";

const editors = new Map<HTMLElement, editor.IStandaloneCodeEditor>();

export async function CreateMonacoEditor(component: DotNet.DotNetObject, el: HTMLElement, value: string, language: language) {
    loader.init().then(i => {
        const edit = i.editor.create(el, {
            value: value,
            language: language,
            theme: 'vs-dark',
            minimap: {
                enabled: false
            },
            automaticLayout: true
        });
        editors.set(el, edit);
        edit.onKeyUp(async _ => {
            const newText = edit.getValue();
            await component.invokeMethodAsync('UpdateValue', newText);
        });
    });
}

export function DisposeMonacoEditor(el: HTMLElement){
    const edit = editors.get(el);
    if(edit){
        edit.dispose();
    }
}