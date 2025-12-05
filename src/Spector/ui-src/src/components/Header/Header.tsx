
import { useSpector } from '../../context/SpectorContext';
import { Button } from '../common/Button';
import styles from './Header.module.css';

export function Header() {
    const { clearAll, togglePause, isPaused, connectionStatus } = useSpector();

    return (
        <header className={styles.header}>
            <div className={styles.headerContent}>
                <h1 className={styles.title}>🔍 Spector</h1>
                <div className={styles.connectionStatus}>
                    <span className={`${styles.statusIndicator} ${styles[connectionStatus]}`} />
                    <span>{connectionStatus === 'connected' ? 'Connected' : connectionStatus === 'disconnected' ? 'Disconnected' : 'Connecting...'}</span>
                </div>
            </div>
            <div className={styles.controls}>
                <Button onClick={clearAll}>Clear All</Button>
                <Button
                    onClick={togglePause}
                    style={{ background: isPaused ? 'var(--accent-orange)' : '' }}
                >
                    {isPaused ? 'Resume' : 'Pause'}
                </Button>
            </div>
        </header>
    );
}
