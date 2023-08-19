function isToday(date: Date) {
    const now = new Date();

    return date.getFullYear() == now.getFullYear()
        && date.getMonth() == now.getMonth()
        && date.getDate() == now.getDate();
}

function zeroPad(value: number) {
    const string = value.toString();

    return string.length === 1
        ? `0${string}`
        : string;
}

export function formatDate(date: Date) {
    return isToday(date)
        ? `${zeroPad(date.getHours())}:${zeroPad(date.getMinutes())}`
        : `${zeroPad(date.getDay())}/${zeroPad(date.getMonth())}`;
}