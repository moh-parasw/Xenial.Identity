import "../css/admin.scss";
import "../../node_modules/monaco-editor/dev/vs/editor/editor.main.css";
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

export function InitMonaco(){
    console.log("InitMonaco", monaco.editor.create);
}
// const MonacoEditor ""