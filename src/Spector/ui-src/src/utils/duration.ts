/**
 * Parse duration string from format "HH:MM:SS.mmmmmmm" to milliseconds
 */
export function parseDuration(duration: string): number {
    const parts = duration.split(':');
    const hours = parseInt(parts[0]);
    const minutes = parseInt(parts[1]);
    const secondsParts = parts[2].split('.');
    const seconds = parseInt(secondsParts[0]);
    const milliseconds = secondsParts[1] ? parseInt(secondsParts[1].substring(0, 3)) : 0;

    return (hours * 3600 + minutes * 60 + seconds) * 1000 + milliseconds;
}

/**
 * Format milliseconds to human-readable duration
 */
export function formatDuration(ms: number): string {
    if (ms < 1) return `${(ms * 1000).toFixed(0)}μs`;
    if (ms < 1000) return `${ms.toFixed(0)}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
}
