import type {ServerLoadEvent} from "@sveltejs/kit";
import {handleProblem, handleProblemForForm} from "$lib/problemDetails";
import {
    type TicketDto, type SwaggerException, type CreateTicketModel, type CreateNoteModel, type EditNoteModel,
    ProjectDto
} from "../../../../../../../gen/planeraClient";
import {getNoteClient, getProjectClient, getTicketClient} from "$lib/clients";
import type {RequestEvent} from "@sveltejs/kit";
import {makeImagePathsAbsolute, makeImagePathsRelative} from "$lib/paths";
import {sanitizeHtml} from "$lib/formatting";

export async function load({ cookies, params }: ServerLoadEvent) {
    let projectResponse: ProjectDto;
    try {
        projectResponse = await getProjectClient(cookies).get(params.user!, params.slug!);
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }

    let ticketResponse: TicketDto;
    try {
        ticketResponse = await getTicketClient(cookies).get(params.user!, params.slug!, Number(params.id!));
    } catch (ex) {
        return handleProblem(ex as SwaggerException);
    }
    
    ticketResponse.description = sanitizeHtml(makeImagePathsAbsolute(ticketResponse.description ?? ""));

    return {
        project: structuredClone(projectResponse),
        ticket: structuredClone(ticketResponse),
    };
}

export const actions = {
    edit: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getTicketClient(cookies).edit(
                formData.get("projectId") as string,
                Number(formData.get("ticketId")),
                {
                    title: formData.get("title") as string,
                    description: makeImagePathsRelative(formData.get("description") as string),
                } as CreateTicketModel,
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException);
        }
    },
    addNote: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getNoteClient(cookies).create({
                    projectId: formData.get("projectId") as string,
                    ticketId: Number(formData.get("ticketId")),
                    content: formData.get("content") as string,
                } as CreateNoteModel,
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException, "addNoteProblem");
        }
    },
    editNote: async ({ request, cookies }: RequestEvent) => {
        const formData = await request.formData();
        try {
            await getNoteClient(cookies).edit(
                Number(formData.get("id")),
                {
                    content: formData.get("content") as string,
                } as EditNoteModel,
            );
        } catch (ex) {
            return handleProblemForForm(ex as SwaggerException, "editNoteProblem");
        }
    }
};