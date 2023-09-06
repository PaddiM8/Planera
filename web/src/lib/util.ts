export function getKeyFromValue(dictionary: {}, key: any) {
    const matchedEntries = Object.entries(dictionary).filter(x => x[1] === key);
    if (matchedEntries.length === 0) {
        return null;
    }

    return matchedEntries[0][0];
}