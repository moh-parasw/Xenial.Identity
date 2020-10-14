import "../css/main.scss";
import { xenial } from "@xenial-io/xenial-template";

xenial();

document.querySelectorAll(".egg").forEach((el: HTMLElement) => {
    el.onclick = async () => {
        window.open("https://unsplash.com/s/photos/austria", "_blank");
    };
});