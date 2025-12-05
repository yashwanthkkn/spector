/**
 * Format JSON string with proper indentation
 * Handles nested JSON strings and wrapper objects
 */
export function formatJson(jsonString: string): string {
    if (!jsonString || jsonString.trim() === '') {
        return '';
    }

    try {
        // First, try to parse the string as JSON
        let parsed = JSON.parse(jsonString);

        // Check if it's a wrapper object with 'content' field
        if (parsed && typeof parsed === 'object' && parsed.content !== undefined) {
            // If content is a string, try to parse it as JSON
            if (typeof parsed.content === 'string') {
                try {
                    const innerParsed = JSON.parse(parsed.content);
                    // Use the inner parsed content instead
                    parsed = innerParsed;
                } catch {
                    // If parsing fails, use the content string as-is
                    return parsed.content;
                }
            } else {
                // If content is already an object, use it
                parsed = parsed.content;
            }
        }

        // Recursively unwrap if there are more nested JSON strings
        if (typeof parsed === 'string') {
            try {
                const innerParsed = JSON.parse(parsed);
                parsed = innerParsed;
            } catch {
                // Not JSON, return as-is
                return parsed;
            }
        }

        // Format the final parsed object
        return JSON.stringify(parsed, null, 2);
    } catch (error) {
        // If parsing fails, return the original string
        return jsonString;
    }
}

/**
 * Escape HTML to prevent XSS
 */
export function escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
