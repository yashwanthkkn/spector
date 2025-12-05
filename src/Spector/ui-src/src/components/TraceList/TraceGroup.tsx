import { useState } from 'react';
import type { Trace } from '../../types';
import { ActivityItem } from './ActivityItem';
import { buildActivityHierarchy } from '../../utils/hierarchy';
import { formatDuration } from '../../utils/duration';
import styles from './TraceGroup.module.css';

interface TraceGroupProps {
    trace: Trace;
}

export function TraceGroup({ trace }: TraceGroupProps) {
    const [isCollapsed, setIsCollapsed] = useState(false);

    const totalDuration = trace.endTime.getTime() - trace.startTime.getTime();
    const activityCount = trace.activities.length;

    const { rootActivities, childrenMap } = buildActivityHierarchy(trace.activities);

    const toggleCollapse = () => {
        setIsCollapsed(!isCollapsed);
    };

    return (
        <div className={styles.traceGroup}>
            <div
                className={`${styles.traceHeader} ${isCollapsed ? styles.collapsed : ''}`}
                onClick={toggleCollapse}
            >
                <div className={styles.traceInfo}>
                    <div className={styles.traceTitle}>Trace</div>
                    <div className={styles.traceId}>{trace.traceId}</div>
                </div>
                <div className={styles.traceMeta}>
                    <span className={styles.traceDuration}>{formatDuration(totalDuration)}</span>
                    <span className={styles.traceCount}>
                        {activityCount} request{activityCount !== 1 ? 's' : ''}
                    </span>
                    <span className={`${styles.collapseIcon} ${isCollapsed ? styles.collapsed : ''}`}>
                        ▼
                    </span>
                </div>
            </div>
            {!isCollapsed && (
                <div className={styles.traceActivities}>
                    {rootActivities.map(activity => (
                        <ActivityItem
                            key={activity.SpanId}
                            activity={activity}
                            children={childrenMap.get(activity.SpanId) || []}
                            traceStartTime={trace.startTime}
                            traceDuration={totalDuration}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
