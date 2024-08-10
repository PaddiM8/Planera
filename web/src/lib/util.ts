import {InterfaceTheme} from "../gen/planeraClient";

export function getKeyFromValue(dictionary: {}, key: any) {
    const matchedEntries = Object.entries(dictionary).filter(x => x[1] === key);
    if (matchedEntries.length === 0) {
        return null;
    }

    return matchedEntries[0][0];
}

export function truncate(value: string, length: number) {
    return value.length > length
        ? value.slice(0, length).trim() + "..."
        : value;
}
