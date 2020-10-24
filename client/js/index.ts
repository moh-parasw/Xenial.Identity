import "../css/main.scss";
import 'notyf/notyf.min.css';
import { xenial } from "@xenial-io/xenial-template";
import QRCode from "qrcode/build/qrcode";
import { Notyf } from "notyf";
xenial();

document.querySelectorAll(".egg").forEach((el: HTMLElement) => {
    el.onclick = async () => {
        window.open("https://unsplash.com/s/photos/austria", "_blank");
    };
});

document.querySelectorAll("[data-qrcode]").forEach((el: HTMLElement) => {
    QRCode.toCanvas(el, el.getAttribute("data-qrcode"), (error) => {
        if (error) {
            console.error(error);
        }
        else {
            console.log('success!');
        }
    });
});

const notyf = new Notyf({
    position: {
        x: "center",
        y: "top"
    },
    duration: 2500,
    dismissible: true
});

document.querySelectorAll("[data-success]").forEach((el: HTMLElement) => {
    notyf.success(el.getAttribute("data-success"));
});

document.querySelectorAll("[data-error]").forEach((el: HTMLElement) => {
    notyf.error(el.getAttribute("data-error"));
});

(window as any).outsideClickHandler = {
    addEvent: (elementId, dotnetHelper) => {
        window.addEventListener("click", async (e: any) => {
            if (!document.getElementById(elementId).contains(e.target)) {
                await dotnetHelper.invokeMethodAsync("InvokeClickOutside");
            } 
        });
    }
};