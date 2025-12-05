import { useMemo } from 'react';
import { useSpector } from '../../context/SpectorContext';
import { TraceGroup } from './TraceGroup';
import { EmptyState } from './EmptyState';
import styles from './TraceList.module.css';

export function TraceList() {
    const { traces } = useSpector();

    const sortedTraces = useMemo(() => {
        return Array.from(traces.values())
            .sort((a, b) => b.startTime.getTime() - a.startTime.getTime());
    }, [traces]);

    if (sortedTraces.length === 0) {
        return (
            <div className={styles.contentArea}>
                <EmptyState />
            </div>
        );
    }

    return (
        <div className={styles.contentArea}>
            <div className={styles.traceList}>
                {sortedTraces.map(trace => (
                    <TraceGroup key={trace.traceId} trace={trace} />
                ))}
            </div>
        </div>
    );
}
