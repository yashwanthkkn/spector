import type { Activity } from '../types';

/**
 * Build parent-child hierarchy from flat activity list
 */
export function buildActivityHierarchy(activities: Activity[]): {
    rootActivities: Activity[];
    childrenMap: Map<string, Activity[]>;
} {
    const rootActivities: Activity[] = [];
    const childrenMap = new Map<string, Activity[]>();
    const activityMap = new Map<string, Activity>();

    // Create activity lookup map
    activities.forEach(activity => {
        activityMap.set(activity.SpanId, activity);
    });

    // Build hierarchy
    activities.forEach(activity => {
        if (!activity.ParentSpanId || !activityMap.has(activity.ParentSpanId)) {
            rootActivities.push(activity);
        } else {
            if (!childrenMap.has(activity.ParentSpanId)) {
                childrenMap.set(activity.ParentSpanId, []);
            }
            childrenMap.get(activity.ParentSpanId)!.push(activity);
        }
    });

    return { rootActivities, childrenMap };
}
