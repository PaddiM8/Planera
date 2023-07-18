import type {SwaggerException} from "../gen/planeraClient";
import {error, fail, redirect} from "@sveltejs/kit";

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
            title: "Error.",
            status: exception.status,
        } as ProblemDetails;
    }
}

export function handleProblem(exception: SwaggerException) {
    const problem = toProblemDetails(exception);
    if (problem.status === 401) {
        throw redirect(302, "/login");
    }

    throw error(problem.status ?? 400, problem.summary);
}

export function handleProblemForForm(exception: SwaggerException) {
    const problem = toProblemDetails(exception);

    return fail(problem.status ?? 400, {
        errors: problem?.errors,
    });
}
