import {TicketPriority} from "../gen/planeraClient";

const priorityNames = [
    "None",
    "Low",
    "Normal",
    "High",
    "Severe",
];

export function priorityToName(priority: TicketPriority): string {
    return priorityNames[priority];
}

export function parsePriority(priorityName: string): TicketPriority {
    return {
        "None": TicketPriority.None,
        "Low": TicketPriority.Low,
        "Normal": TicketPriority.Normal,
        "High": TicketPriority.High,
        "Severe": TicketPriority.Severe,
    }[priorityName] ?? TicketPriority.None;
}