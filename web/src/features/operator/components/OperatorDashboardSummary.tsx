import { Card } from '@/components/ui/Card'
import type { DashboardSummaryResponse } from '@/features/operator/types/operatorTypes'

export interface OperatorDashboardSummaryProps {
  summary: DashboardSummaryResponse
}

export function OperatorDashboardSummary({ summary }: OperatorDashboardSummaryProps) {
  return (
    <div className="operator-summary" aria-label="Assigned microgrid node reservation summary">
      <Card className="operator-summary__card" title="Pending Reservations">
        <p className="operator-summary__value">{summary.pendingReservationCount}</p>
        <p className="operator-summary__description">Reservations awaiting Backoffice approval at your assigned microgrid node.</p>
      </Card>
      <Card className="operator-summary__card" title="Approved Future Reservations">
        <p className="operator-summary__value">{summary.approvedFutureReservationCount}</p>
        <p className="operator-summary__description">Approved reservations scheduled in the future at your assigned microgrid node.</p>
      </Card>
    </div>
  )
}
