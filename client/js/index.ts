import "../css/main.scss";
import { xenial } from "@xenial-io/xenial-template";
import QRCode from "qrcode/build/qrcode";

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

