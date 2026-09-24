import type { HTMLAttributes, ReactNode } from 'react'

export type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info'

export interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  children: ReactNode
  variant?: BadgeVariant
}

export function Badge({ children, variant = 'default', className = '', ...props }: BadgeProps) {
  return <span className={['badge', `badge--${variant}`, className].filter(Boolean).join(' ')} {...props}>{children}</span>
}
