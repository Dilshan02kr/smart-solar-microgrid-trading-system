import type { ReactNode } from 'react'

export interface PageHeaderProps {
  title: string
  description?: string
  actions?: ReactNode
}

export function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <header className="page-header">
      <div>
        <h1 className="page-header__title">{title}</h1>
        {description && <p className="page-header__description">{description}</p>}
      </div>
      {actions && <div className="page-header__actions">{actions}</div>}
    </header>
  )
}

export interface PageContentProps {
  children: ReactNode
  className?: string
}

export function PageContent({ children, className = '' }: PageContentProps) {
  return <div className={['page-content', className].filter(Boolean).join(' ')}>{children}</div>
}
