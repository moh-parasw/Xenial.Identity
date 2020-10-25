import { notyf } from "./notify";

const dropArea = document.querySelector(".edit-picture-page")

if (dropArea && dropArea instanceof HTMLElement) {
    const preventDefaults = (e: Event) => {
        e.preventDefault();
        e.stopPropagation();
    }

    const highlight = () => {
        dropArea.classList.add('highlight');
    }

    const unhighlight = () => {
        dropArea.classList.remove('highlight');
    }

    const handleDrop = (e: DragEvent) => {
        const dt = e.dataTransfer;
        if (dt) {
            handleFiles(dt.files);
        }
    }

    const uploadProgress: number[] = [];

    const initializeProgress = (numFiles: number) => {
        uploadProgress.length = 0;

        for (let i = numFiles; i > 0; i--) {
            uploadProgress.push(0)
        }
    }

    const handleFiles = (files: FileList) => {
        initializeProgress(files.length);
        const files2: File[] = [];
        for (let index = 0; index < files.length; index++) {
            const element = files[index];
            files2.push(element);
        }

        files2.forEach(uploadFile);
        files2.forEach(previewFile);
    }

    const previewFile = (file: File) => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = () => {
            const img: HTMLImageElement = document.createElement('img');
            if (reader.result) {
                img.src = reader.result.toString();
            }
            // document.getElementById('gallery').appendChild(img);
        }
    }

    const uploadFile = async (file: File, i: number) => {
        const formData = new FormData();

        const form = dropArea.querySelector("#upload-form");

        if (form instanceof HTMLFormElement) {

            const requestVerificationToken = form.querySelector("input[name='__RequestVerificationToken']");

            if (requestVerificationToken) {

                formData.append('Upload', file);
                formData.append('__RequestVerificationToken', requestVerificationToken.getAttribute("value")!);

                try {
                    const response = await fetch(form.action, {
                        method: 'POST',
                        body: formData,
                        headers: {
                            'Accept': 'application/json'
                        }
                    });

                    const payload = await response.json();

                    if (response.ok) {
                        if (payload.statusMessage) {
                            notyf.success(payload.statusMessage);
                        }
                        if (payload.imageUri) {
                            const imgElement = dropArea.querySelector(".edit-picture-page__image");
                            if (imgElement && imgElement instanceof HTMLImageElement) {
                                imgElement.src = payload.imageUri;
                            }
                        }
                    }
                    else {
                        if (payload.statusMessage) {
                            notyf.error(payload.statusMessage);
                        }
                        console.error(payload);
                    }
                }

                catch (e) {
                    notyf.error(e);
                    console.error(e);
                }
            }
        }
    }

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, preventDefaults, false)
        document.body.addEventListener(eventName, preventDefaults, false)
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropArea.addEventListener(eventName, highlight, false)
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, unhighlight, false)
    });

    dropArea.addEventListener('drop', handleDrop, false);
}
