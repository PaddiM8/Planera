import type {SwaggerException} from "../gen/planeraClient";

interface ProblemDetails {
    title?: string;
    status?: number;
    errors?: { string: Array<string> };
}

export function toProblemDetails(exception: SwaggerException) {
    try {
        return JSON.parse(exception.response) as ProblemDetails;
    } catch {
        return {
            title: "Internal server error.",
            status: 500,
        } as ProblemDetails;
    }
}