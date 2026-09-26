import type { HTMLAttributes, ReactNode } from 'react'

export interface CardProps extends HTMLAttributes<HTMLElement> {
  children: ReactNode
  title?: string
  description?: string
}

export function Card({ children, title, description, className = '', ...props }: CardProps) {
  return (
    <section className={['card', className].filter(Boolean).join(' ')} {...props}>
      {title && <h2 className="card__title">{title}</h2>}
      {description && <p className="card__description">{description}</p>}
      {children}
    </section>
  )
}
