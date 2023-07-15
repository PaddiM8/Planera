import type {SwaggerException} from "../gen/planeraClient";

interface ProblemDetails {
    title?: string;
    status?: number;
    errors?: { string: string[] };
    summary?: string,
}

export function toProblemDetails(exception: SwaggerException) {
    try {
        const problem = JSON.parse(exception.response) as ProblemDetails;
        problem.summary = Object.values(problem.errors ?? []).at(0)?.at(0) || problem.title;

        return problem;
    } catch {
        return {
            title: "Internal server error.",
            status: 500,
        } as ProblemDetails;
    }
}