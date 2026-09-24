import type { ReactNode } from 'react'

export type AlertVariant = 'success' | 'error' | 'warning' | 'info'

export interface AlertProps {
  children: ReactNode
  title?: string
  variant?: AlertVariant
}

const marks: Record<AlertVariant, string> = { success: '✓', error: '!', warning: '!', info: 'i' }

export function Alert({ children, title, variant = 'info' }: AlertProps) {
  return (
    <div className={`alert alert--${variant}`} role={variant === 'error' ? 'alert' : 'status'}>
      <span className="alert__mark" aria-hidden="true">{marks[variant]}</span>
      <div className="alert__content">
        {title && <h3 className="alert__title">{title}</h3>}
        <p className="alert__message">{children}</p>
      </div>
    </div>
  )
}
