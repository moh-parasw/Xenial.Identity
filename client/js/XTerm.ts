import "../css/XTerm.scss";
import "xterm/css/xterm.css";
import { Terminal } from "xterm";

const terminals = new Map<HTMLElement, Terminal>();

export function CreateTerminal(el: HTMLElement) {
    const terminal = new Terminal();
    terminal.open(el);
    terminals.set(el, terminal);
}

export function WriteTerminal(el: HTMLElement, buffer: string | Uint8Array){
    terminals.get(el)?.write(buffer);
}

export function WriteLineTerminal(el:HTMLElement, buffer: string | Uint8Array){
    terminals.get(el)?.writeln(buffer);
}

export function DisposeTerminal(el: HTMLElement){
    terminals.get(el)?.dispose();
    terminals.delete(el);
}
