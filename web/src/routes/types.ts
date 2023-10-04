export type FormSubmitInput = {
    action: URL;
    data: FormData;
    formData: FormData;
    form: HTMLFormElement;
    formElement: HTMLFormElement;
    controller: AbortController;
    submitter: HTMLElement | null;
    cancel(): void;
};

