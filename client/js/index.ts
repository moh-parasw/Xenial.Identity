import "../css/main.scss";
import 'notyf/notyf.min.css';

import { xenial } from "@xenial-io/xenial-template";
import QRCode from "qrcode";
import { notyf } from "./notify";
import { MvcGrid } from "./components/mvc-grid-6-2-1/mvc-grid";
import "./file-upload";
import copy from "copy-to-clipboard";
import Tagify from '@yaireo/tagify'

xenial();

document.querySelectorAll(".egg").forEach((el) => {
    if (el instanceof HTMLElement) {
        el.onclick = async () => {
            window.open("https://unsplash.com/s/photos/austria", "_blank");
        };
    }
});

document.querySelectorAll("[data-qrcode]").forEach((el) => {
    QRCode.toCanvas(el, el.getAttribute("data-qrcode"), (error: string | Error) => {
        if (error) {
            console.error(error);
        }
        else {
            console.log('success!');
        }
    });
});

document.querySelectorAll("[data-success]").forEach((el) => {
    notyf.success(el.getAttribute("data-success")!);
});

document.querySelectorAll("[data-error]").forEach((el) => {
    notyf.error(el.getAttribute("data-error")!);
});

(window as any).outsideClickHandler = {
    addEvent: (elementId: string, dotnetHelper: any) => {
        window.addEventListener("click", async (e: any) => {
            const el = document.getElementById(elementId);
            if (el && !el.contains(e.target)) {
                await dotnetHelper.invokeMethodAsync("InvokeClickOutside");
            }
        });
    }
};

document.querySelectorAll(".mvc-grid").forEach(element => {
    if (element instanceof HTMLElement) {
        new MvcGrid(element);
    }
});
document.querySelectorAll("[data-copy]").forEach(element => {
    if (element instanceof HTMLElement) {
        element.style.cursor = "pointer";
        const dataToCopy = element.getAttribute("data-copy");
        element.title = `copy ${dataToCopy} to clipboard`;
        element.onclick = () => {
            const dataToCopy = element.getAttribute("data-copy");
            if (dataToCopy) {
                copy(dataToCopy);
                notyf.success(`<div style="text-align: center;">Copied <br>${dataToCopy}<br>to clipboard</div>`)
            }
        };
    }
});

document.querySelectorAll("[data-tags]").forEach(element => {
    if (element instanceof HTMLElement) {
        const dataTags = element.getAttribute("data-tags")?.split(",")?.map(s => s.trim());
        if (dataTags) {
            const isSimple = element.getAttribute("data-tags-simple");
            new Tagify(element, {
                whitelist: dataTags,
                dropdown: {
                    maxItems: 99,           // <- mixumum allowed rendered suggestions
                    enabled: 0,             // <- show suggestions on focus
                    closeOnSelect: true    // <- do not hide the suggestions dropdown once an item has been selected
                },
                originalInputValueFormat: 
                    isSimple 
                    ? (valuesArr: any[]) => valuesArr.map(item => item.value).join(',')
                    : undefined
            });
        }
    }
});

document.querySelectorAll("[data-redirect]").forEach(element => {
    if (element instanceof HTMLLinkElement) {
        window.location.href = element.href;
    }
});
