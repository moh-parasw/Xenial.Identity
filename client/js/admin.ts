import "../css/admin.scss";
import "../../node_modules/monaco-editor/dev/vs/editor/editor.main.css";
import MonacoEditor from "monaco-editor";

export function InitMonaco(){
    console.log("InitMonaco", MonacoEditor.editor.create);
}
// const MonacoEditor ""