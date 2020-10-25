import { Notyf } from "notyf";

const notyf = new Notyf({
    position: {
        x: "center",
        y: "top"
    },
    duration: 2500,
    dismissible: true
});

export {
    notyf
}