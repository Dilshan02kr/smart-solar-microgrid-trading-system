import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { Spinner } from '@/components/feedback/Spinner'

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'danger' | 'ghost'

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode
  variant?: ButtonVariant
  loading?: boolean
}

export function Button({ children, variant = 'primary', loading = false, disabled, className = '', type = 'button', ...props }: ButtonProps) {
  const classes = ['button', `button--${variant}`, className].filter(Boolean).join(' ')

  return (
    <button className={classes} type={type} disabled={disabled || loading} aria-busy={loading || undefined} {...props}>
      {loading && <Spinner size="small" label="" />}
      <span>{children}</span>
    </button>
  )
}
