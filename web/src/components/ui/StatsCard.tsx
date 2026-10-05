import type { ReactNode } from 'react'
import { Card } from '@/components/ui/Card'

export interface StatsCardProps {
  label: string
  value: number | string
  detail: string
  tone?: 'primary' | 'warning' | 'success' | 'info'
  action?: ReactNode
}

export function StatsCard({ label, value, detail, tone = 'primary', action }: StatsCardProps) {
  return (
    <Card className={`stats-card stats-card--${tone}`}>
      <p className="stats-card__label">{label}</p>
      <p className="stats-card__value">{value}</p>
      <p className="stats-card__detail">{detail}</p>
      {action && <div className="stats-card__action">{action}</div>}
    </Card>
  )
}
