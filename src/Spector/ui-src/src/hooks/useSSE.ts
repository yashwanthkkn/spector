import { useEffect, useRef } from 'react';
import type { Activity, ConnectionStatus } from '../types';

interface UseSSEOptions {
    url: string;
    onMessage: (activity: Activity) => void;
    onStatusChange: (status: ConnectionStatus) => void;
    isPaused: boolean;
}

export function useSSE({ url, onMessage, onStatusChange, isPaused }: UseSSEOptions) {
    const eventSourceRef = useRef<EventSource | null>(null);
    const reconnectTimeoutRef = useRef<number | null>(null);

    useEffect(() => {
        function connect() {
            // Clean up existing connection
            if (eventSourceRef.current) {
                eventSourceRef.current.close();
            }

            const eventSource = new EventSource(url);
            eventSourceRef.current = eventSource;

            eventSource.onopen = () => {
                onStatusChange('connected');
                console.log('SSE Connected');
            };

            eventSource.onmessage = (event) => {
                if (isPaused) return;

                try {
                    const activity: Activity = JSON.parse(event.data);
                    console.log(activity);
                    onMessage(activity);
                } catch (error) {
                    console.error('Error parsing SSE data:', error);
                }
            };

            eventSource.onerror = () => {
                onStatusChange('disconnected');
                console.error('SSE Error');

                // Attempt to reconnect after 3 seconds
                if (reconnectTimeoutRef.current) {
                    clearTimeout(reconnectTimeoutRef.current);
                }

                reconnectTimeoutRef.current = window.setTimeout(() => {
                    if (eventSource.readyState === EventSource.CLOSED) {
                        connect();
                    }
                }, 3000);
            };
        }

        connect();

        // Cleanup on unmount
        return () => {
            if (eventSourceRef.current) {
                eventSourceRef.current.close();
            }
            if (reconnectTimeoutRef.current) {
                clearTimeout(reconnectTimeoutRef.current);
            }
        };
    }, [url, onMessage, onStatusChange, isPaused]);
}
