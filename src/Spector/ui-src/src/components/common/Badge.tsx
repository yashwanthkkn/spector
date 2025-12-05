import React from 'react';
import styles from './Badge.module.css';

interface BadgeProps {
    type: 'httpin' | 'httpout';
    children: React.ReactNode;
}

export function Badge({ type, children }: BadgeProps) {
    return (
        <span className={`${styles.badge} ${styles[type]}`}>
            {children}
        </span>
    );
}
