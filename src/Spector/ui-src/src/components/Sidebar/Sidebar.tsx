
import { useSpector } from '../../context/SpectorContext';
import styles from './Sidebar.module.css';

export function Sidebar() {
    const { filters, setFilter, traces, activities } = useSpector();

    return (
        <aside className={styles.sidebar}>
            <div className={styles.filterSection}>
                <h3>Filters</h3>
                <div className={styles.filterGroup}>
                    <label>
                        <input
                            type="checkbox"
                            checked={filters.httpIn}
                            onChange={(e) => setFilter('httpIn', e.target.checked)}
                        />
                        <span className={`${styles.filterLabel} ${styles.httpIn}`}>HTTP In</span>
                    </label>
                    <label>
                        <input
                            type="checkbox"
                            checked={filters.httpOut}
                            onChange={(e) => setFilter('httpOut', e.target.checked)}
                        />
                        <span className={`${styles.filterLabel} ${styles.httpOut}`}>HTTP Out</span>
                    </label>
                </div>
            </div>

            <div className={styles.statsSection}>
                <h3>Statistics</h3>
                <div className={styles.statItem}>
                    <span className={styles.statLabel}>Total Requests:</span>
                    <span className={styles.statValue}>{activities.size}</span>
                </div>
                <div className={styles.statItem}>
                    <span className={styles.statLabel}>Active Traces:</span>
                    <span className={styles.statValue}>{traces.size}</span>
                </div>
            </div>
        </aside>
    );
}
