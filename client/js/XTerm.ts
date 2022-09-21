import "../css/XTerm.scss";
import "xterm/css/xterm.css";
import { Terminal } from "xterm";
import { FitAddon } from 'xterm-addon-fit';

const terminals = new Map<HTMLElement, Terminal>();
const fitAddon = new FitAddon();
export function CreateTerminal(el: HTMLElement, cols?: number, rows?: number, autoFit?: boolean) {
    const terminal = new Terminal({
        cols,
        rows,
    });

    terminal.loadAddon(fitAddon);
    terminal.open(el);
    terminals.set(el, terminal);
    if (autoFit) {
        fitAddon.fit();
    }
}

export function ResizeTerminal(el: HTMLElement, cols?: number, rows?: number) {
    const terminal = terminals.get(el);
    if (terminal) {
        terminal.resize(cols ?? terminal.cols, rows ?? terminal.rows);
    }
}

export function FitTerminal(el: HTMLElement) {
    fitAddon.fit();
}

export function ClearTerminal(el: HTMLElement) {
    terminals.get(el)?.clear();
}

export function ScrollTerminalToBottom(el: HTMLElement) {
    terminals.get(el)?.scrollToBottom();
}

export function WriteTerminal(el: HTMLElement, buffer: string | Uint8Array) {
    terminals.get(el)?.write(buffer);
}

export function WriteLineTerminal(el: HTMLElement, buffer: string | Uint8Array) {
    terminals.get(el)?.writeln(buffer);
}

export function DisposeTerminal(el: HTMLElement) {
    terminals.get(el)?.dispose();
    terminals.delete(el);
}
