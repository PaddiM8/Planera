import type {SwaggerException} from "../gen/planeraClient";
import {error, fail, redirect} from "@sveltejs/kit";

export interface ProblemDetails {
    title?: string;
    status?: number;
    errors?: { [key: string]: string[] };
    summary?: string,
}

export function toProblemDetails(exception: SwaggerException) {
    try {
        const problem = JSON.parse(exception.response) as ProblemDetails;
        problem.summary = Object.values(problem.errors ?? []).at(0)?.at(0) || problem.title;

        if (problem.errors) {
            for (const problemKey in problem.errors) {
                // If there are several errors for a field and one of them is
                // a "field is required" error, ignore the "field is required" error.
                const errorList = problem.errors[problemKey];
                problem.errors[problemKey] = errorList.filter(x =>
                    errorList.length === 1 || !x.endsWith(" field is required.")
                );
            }
        }

        return problem;
    } catch {
        return {
            title: "Error.",
            status: exception.status,
            summary: "An unknown error occured",
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

export function handleProblemForForm(exception: SwaggerException, fieldName = "problem") {
    const problem = toProblemDetails(exception);
    const data: any = {};
    data[fieldName] = problem;

    return fail(problem.status ?? 400, data);
}
