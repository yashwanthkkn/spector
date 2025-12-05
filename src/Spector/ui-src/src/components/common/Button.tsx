import React from 'react';
import styles from './Button.module.css';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary';
    children: React.ReactNode;
}

export function Button({ variant = 'secondary', children, className, ...props }: ButtonProps) {
    return (
        <button
            className={`${styles.btn} ${styles[variant]} ${className || ''}`}
            {...props}
        >
            {children}
        </button>
    );
}
