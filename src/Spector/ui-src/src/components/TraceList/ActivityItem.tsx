import React from 'react';
import type { Activity } from '../../types';
import { useSpector } from '../../context/SpectorContext';
import { parseDuration, formatDuration } from '../../utils/duration';
import styles from './ActivityItem.module.css';

interface ActivityItemProps {
    activity: Activity;
    children?: Activity[];
    traceStartTime: Date;
    traceDuration: number;
    isChild?: boolean;
}

export function ActivityItem({
    activity,
    children = [],
    traceStartTime,
    traceDuration,
    isChild = false
}: ActivityItemProps) {
    const { selectActivity, selectedActivity, filters } = useSpector();

    const type = activity.Name.toLowerCase();
    const method = activity.Tags['spector.method'] || 'N/A';
    const url = activity.Tags['spector.url'] || 'N/A';
    const status = activity.Tags['spector.status'] || '';
    const error = activity.Tags['spector.error'] || '';
    const duration = parseDuration(activity.Duration);

    // Check filters
    if (type === 'httpin' && !filters.httpIn) return null;
    if (type === 'httpout' && !filters.httpOut) return null;

    // Calculate timeline position and width
    const timelineWidth = traceDuration > 0 ? (duration / traceDuration) * 100 : 100;

    // Determine status color
    let statusClass = '';
    let statusDisplay = status;

    if (error || status === '0') {
        statusClass = styles.statusError;
        statusDisplay = 'ERROR';
    } else if (status) {
        const statusCode = parseInt(status);
        if (statusCode >= 200 && statusCode < 300) statusClass = styles.statusSuccess;
        else if (statusCode >= 300 && statusCode < 400) statusClass = styles.statusRedirect;
        else if (statusCode >= 400 && statusCode < 500) statusClass = styles.statusClientError;
        else if (statusCode >= 500) statusClass = styles.statusServerError;
    }

    const isSelected = selectedActivity?.SpanId === activity.SpanId;

    const handleClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        selectActivity(activity.SpanId);
    };

    return (
        <>
            <div
                className={`${styles.activityItem} ${isChild ? styles.child : ''} ${isSelected ? styles.selected : ''}`}
                onClick={handleClick}
            >
                <span className={`${styles.activityType} ${styles[type]}`} />
                <div className={styles.activityContent}>
                    <div className={styles.activityName}>
                        <span className={`${styles.activityMethod} ${styles[`method${method}`]}`}>{method}</span>
                        {status && <span className={`${styles.activityStatus} ${statusClass}`}>{statusDisplay}</span>}
                    </div>
                    <div className={styles.activityUrl}>{url}</div>
                </div>
                <div className={styles.activityTiming}>
                    <span className={styles.activityDuration}>{formatDuration(duration)}</span>
                    <div className={styles.activityTimeline}>
                        <div className={styles.timelineBar} style={{ width: `${timelineWidth}%` }} />
                    </div>
                </div>
            </div>
            {children.map(child => (
                <ActivityItem
                    key={child.SpanId}
                    activity={child}
                    children={[]}
                    traceStartTime={traceStartTime}
                    traceDuration={traceDuration}
                    isChild={true}
                />
            ))}
        </>
    );
}
