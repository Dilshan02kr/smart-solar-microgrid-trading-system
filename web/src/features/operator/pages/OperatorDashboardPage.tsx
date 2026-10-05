import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Button } from '@/components/ui/Button'
import { getOperatorDashboardSummary } from '@/features/operator/api/operatorApi'
import { OperatorDashboardSummary } from '@/features/operator/components/OperatorDashboardSummary'
import type { DashboardSummaryResponse } from '@/features/operator/types/operatorTypes'
import { getOperatorErrorMessage } from '@/features/operator/utils/operatorPresentation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function OperatorDashboardPage() {
  const [summary, setSummary] = useState<DashboardSummaryResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)

  function refresh() {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    getOperatorDashboardSummary(controller.signal)
      .then((response) => {
        setSummary(response)
        setError(null)
      })
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  const unassigned = error?.code === 'OPERATOR_STATION_NOT_ASSIGNED'

  return (
    <>
      <PageHeader
        title="Grid Operator Dashboard"
        description="View reservation activity scoped by the server to your assigned microgrid node."
        actions={<><Button variant="outline" onClick={refresh} disabled={isLoading}>Refresh</Button><Link className="button button--outline" to="/operator/slots">Energy Slots</Link><Link className="button button--outline" to="/operator/reservations">View Reservations</Link><Link className="button button--primary" to="/operator/operations">Verify Transaction</Link></>}
      />
      <PageContent>
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading operator dashboard" /></div>
        ) : unassigned ? (
          <Alert variant="warning" title="Microgrid node assignment required">{getOperatorErrorMessage(error)}</Alert>
        ) : error ? (
          <PageError message={error.message} onRetry={refresh} />
        ) : summary ? (
          <OperatorDashboardSummary summary={summary} />
        ) : null}
      </PageContent>
    </>
  )
}
