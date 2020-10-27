import "../css/main.scss";
import 'notyf/notyf.min.css';
import { xenial } from "@xenial-io/xenial-template";
import QRCode from "qrcode/build/qrcode";
import { notyf } from "./notify";
import { MvcGrid } from "./components/mvc-grid-6-2-1/mvc-grid";
import "./file-upload";

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