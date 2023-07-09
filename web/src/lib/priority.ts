import {Priority} from "../gen/planeraClient";

const priorityNames = [
    "None",
    "Low",
    "Normal",
    "High",
    "Severe",
];

export function priorityToName(priority: Priority, noneAsEmpty = false): string {
    return priority == Priority.None && noneAsEmpty
        ? ""
        : priorityNames[priority];
}

export function parsePriority(priorityName: string): Priority {
    return {
        "None": Priority.None,
        "Low": Priority.Low,
        "Normal": Priority.Normal,
        "High": Priority.High,
        "Severe": Priority.Severe,
    }[priorityName] ?? Priority.None;
}