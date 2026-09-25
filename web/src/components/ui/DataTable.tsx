import type { ReactNode, TableHTMLAttributes } from 'react'

export interface DataTableProps extends TableHTMLAttributes<HTMLTableElement> {
  caption: string
  children: ReactNode
}

export function DataTable({ caption, children, className = '', ...props }: DataTableProps) {
  return (
    <div className="table-scroll" tabIndex={0} role="region" aria-label={caption}>
      <table className={['data-table', className].filter(Boolean).join(' ')} {...props}>
        <caption className="sr-only">{caption}</caption>
        {children}
      </table>
    </div>
  )
}
