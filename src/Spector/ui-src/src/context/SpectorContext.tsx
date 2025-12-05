import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import type { Activity, Trace, Filters, ConnectionStatus } from '../types';
import { parseDuration } from '../utils/duration';

interface SpectorState {
    traces: Map<string, Trace>;
    activities: Map<string, Activity>;
    filters: Filters;
    isPaused: boolean;
    selectedActivity: Activity | null;
    connectionStatus: ConnectionStatus;
}

interface SpectorContextType extends SpectorState {
    addActivity: (activity: Activity) => void;
    clearAll: () => void;
    togglePause: () => void;
    selectActivity: (spanId: string) => void;
    closeDetails: () => void;
    setFilter: (filter: keyof Filters, value: boolean) => void;
    setConnectionStatus: (status: ConnectionStatus) => void;
}

const SpectorContext = createContext<SpectorContextType | undefined>(undefined);

export function SpectorProvider({ children }: { children: ReactNode }) {
    const [traces, setTraces] = useState<Map<string, Trace>>(new Map());
    const [activities, setActivities] = useState<Map<string, Activity>>(new Map());
    const [filters, setFilters] = useState<Filters>({ httpIn: true, httpOut: true });
    const [isPaused, setIsPaused] = useState(false);
    const [selectedActivity, setSelectedActivity] = useState<Activity | null>(null);
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting');

    const addActivity = useCallback((activity: Activity) => {
        const { TraceId, SpanId } = activity;

        // Store activity
        setActivities(prev => new Map(prev).set(SpanId, activity));

        // Get or create trace group
        setTraces(prev => {
            const newTraces = new Map(prev);

            if (!newTraces.has(TraceId)) {
                newTraces.set(TraceId, {
                    traceId: TraceId,
                    activities: [],
                    startTime: new Date(activity.StartTimeUtc),
                    endTime: new Date(activity.StartTimeUtc)
                });
            }

            const trace = newTraces.get(TraceId)!;
            trace.activities.push(activity);

            // Update trace timing
            const activityStart = new Date(activity.StartTimeUtc);
            const activityEnd = new Date(activityStart.getTime() + parseDuration(activity.Duration));

            if (activityStart < trace.startTime) trace.startTime = activityStart;
            if (activityEnd > trace.endTime) trace.endTime = activityEnd;

            return newTraces;
        });
    }, []);

    const clearAll = useCallback(() => {
        setTraces(new Map());
        setActivities(new Map());
        setSelectedActivity(null);
    }, []);

    const togglePause = useCallback(() => {
        setIsPaused(prev => !prev);
    }, []);

    const selectActivity = useCallback((spanId: string) => {
        setActivities(prev => {
            const activity = prev.get(spanId);
            if (activity) {
                setSelectedActivity(activity);
            }
            return prev;
        });
    }, []);

    const closeDetails = useCallback(() => {
        setSelectedActivity(null);
    }, []);

    const setFilter = useCallback((filter: keyof Filters, value: boolean) => {
        setFilters(prev => ({ ...prev, [filter]: value }));
    }, []);

    const value: SpectorContextType = {
        traces,
        activities,
        filters,
        isPaused,
        selectedActivity,
        connectionStatus,
        addActivity,
        clearAll,
        togglePause,
        selectActivity,
        closeDetails,
        setFilter,
        setConnectionStatus
    };

    return <SpectorContext.Provider value={value}>{children}</SpectorContext.Provider>;
}

export function useSpector() {
    const context = useContext(SpectorContext);
    if (context === undefined) {
        throw new Error('useSpector must be used within a SpectorProvider');
    }
    return context;
}
