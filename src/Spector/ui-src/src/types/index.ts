// Activity data structure from SSE
export interface Activity {
    TraceId: string;
    SpanId: string;
    ParentSpanId: string | null;
    Name: string;
    StartTimeUtc: string;
    Duration: string;
    Tags: Record<string, string>;
}

// Trace group containing multiple activities
export interface Trace {
    traceId: string;
    activities: Activity[];
    startTime: Date;
    endTime: Date;
}

// Filter state
export interface Filters {
    httpIn: boolean;
    httpOut: boolean;
}

// Connection status
export type ConnectionStatus = 'connected' | 'disconnected' | 'connecting';

// Activity type
export type ActivityType = 'httpin' | 'httpout';

// HTTP methods
export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' | 'OPTIONS' | 'HEAD';

// Status code ranges
export type StatusType = 'success' | 'redirect' | 'client-error' | 'server-error' | 'error';
