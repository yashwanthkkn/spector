import { useSpector } from '../../context/SpectorContext';
import { parseDuration, formatDuration } from '../../utils/duration';
import { formatJson } from '../../utils/formatting';
import { Badge } from '../common/Badge';
import styles from './DetailsPanel.module.css';

export function DetailsPanel() {
    const { selectedActivity, closeDetails } = useSpector();

    if (!selectedActivity) {
        return (
            <aside className={styles.detailsPanel}>
                <div className={styles.detailsHeader}>
                    <h3>Request Details</h3>
                </div>
                <div className={styles.detailsContent}>
                    <p className={styles.detailsPlaceholder}>Select a request to view details</p>
                </div>
            </aside>
        );
    }

    const type = selectedActivity.Name.toLowerCase() as 'httpin' | 'httpout';
    const method = selectedActivity.Tags['spector.method'] || 'N/A';
    const url = selectedActivity.Tags['spector.url'] || 'N/A';
    const status = selectedActivity.Tags['spector.status'] || '';
    const error = selectedActivity.Tags['spector.error'] || '';
    const errorType = selectedActivity.Tags['spector.errorType'] || '';
    const duration = parseDuration(selectedActivity.Duration);
    const requestBody = selectedActivity.Tags['spector.requestBody'] || '';
    const responseBody = selectedActivity.Tags['spector.responseBody'] || '';

    return (
        <aside className={`${styles.detailsPanel} ${styles.open}`}>
            <div className={styles.detailsHeader}>
                <h3>Request Details</h3>
                <button className={styles.closeBtn} onClick={closeDetails}>×</button>
            </div>
            <div className={styles.detailsContent}>
                <div className={styles.detailSection}>
                    <h4>Overview</h4>
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>Type</span>
                        <span className={styles.detailValue}>
                            <Badge type={type}>{selectedActivity.Name}</Badge>
                        </span>
                    </div>
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>Method</span>
                        <span className={styles.detailValue}>
                            <span className={`${styles.activityMethod} ${styles[`method${method}`]}`}>{method}</span>
                        </span>
                    </div>
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>URL</span>
                        <span className={styles.detailValue}>{url}</span>
                    </div>
                    {status && status !== '0' && (
                        <div className={styles.detailRow}>
                            <span className={styles.detailLabel}>Status</span>
                            <span className={styles.detailValue}>{status}</span>
                        </div>
                    )}
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>Duration</span>
                        <span className={styles.detailValue}>{formatDuration(duration)}</span>
                    </div>
                </div>

                {error && (
                    <div className={styles.detailSection}>
                        <h4>Error Details</h4>
                        <div className={styles.detailRow}>
                            <span className={styles.detailLabel}>Error Type</span>
                            <span className={styles.detailValue}>{errorType}</span>
                        </div>
                        <div className={styles.detailRow}>
                            <span className={styles.detailLabel}>Error Message</span>
                            <span className={styles.detailValue}>{error}</span>
                        </div>
                    </div>
                )}

                <div className={styles.detailSection}>
                    <h4>Timing</h4>
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>Start Time</span>
                        <span className={styles.detailValue}>
                            {new Date(selectedActivity.StartTimeUtc).toLocaleString()}
                        </span>
                    </div>
                    <div className={styles.detailRow}>
                        <span className={styles.detailLabel}>Duration</span>
                        <span className={styles.detailValue}>{selectedActivity.Duration}</span>
                    </div>
                </div>

                {requestBody && (
                    <div className={styles.detailSection}>
                        <h4>Request Body</h4>
                        <div className={styles.codeBlock}>
                            <pre>{formatJson(requestBody)}</pre>
                        </div>
                    </div>
                )}

                {responseBody && (
                    <div className={styles.detailSection}>
                        <h4>Response Body</h4>
                        <div className={styles.codeBlock}>
                            <pre>{formatJson(responseBody)}</pre>
                        </div>
                    </div>
                )}
            </div>
        </aside>
    );
}
