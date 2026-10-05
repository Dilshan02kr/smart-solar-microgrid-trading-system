import type { ReactNode } from 'react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'

export interface FilterBarProps {
  children: ReactNode
  onClear: () => void
  resultCount?: number
  title?: string
}

export function FilterBar({ children, onClear, resultCount, title = 'Search and filters' }: FilterBarProps) {
  return (
    <Card className="filter-bar" aria-label={title}>
      <div className="filter-bar__heading">
        <div>
          <h2>{title}</h2>
          {resultCount !== undefined && <p aria-live="polite">{resultCount} {resultCount === 1 ? 'result' : 'results'}</p>}
        </div>
        <Button type="button" variant="ghost" onClick={onClear}>Clear filters</Button>
      </div>
      <div className="filter-bar__controls">{children}</div>
    </Card>
  )
}
