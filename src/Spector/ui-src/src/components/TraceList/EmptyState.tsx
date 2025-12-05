
import styles from './EmptyState.module.css';

export function EmptyState() {
    return (
        <div className={styles.emptyState}>
            <div className={styles.emptyIcon}>📡</div>
            <p>Waiting for network activity...</p>
            <small>Make API requests to see them appear here</small>
        </div>
    );
}
