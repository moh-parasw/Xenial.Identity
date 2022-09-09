import "../css/admin.scss";
import loader from '@monaco-editor/loader';

type language = "html" | "json" | "css";

export async function CreateMonacoEditor(el: HTMLElement, value: string, language: language) {
    loader.init().then(i => {
        const edit = i.editor.create(el, {
            value: value,
            language: language
        });
        edit.onDidChangeModel(e => {
            const newText = edit.getModel()?.getValue();
            console.warn(newText);
        })
    });

}