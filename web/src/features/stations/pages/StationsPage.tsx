import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { activateStation, deactivateStation, getStations } from '@/features/stations/api/stationsApi'
import { StationsTable } from '@/features/stations/components/StationsTable'
import { StationStatus, type Station } from '@/features/stations/types/stationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function StationsPage() {
  const [stations, setStations] = useState<Station[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [selectedStation, setSelectedStation] = useState<Station | null>(null)
  const [isUpdating, setIsUpdating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  const retry = useCallback(() => {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    getStations(controller.signal)
      .then(setStations)
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  async function handleStatusChange() {
    if (!selectedStation || isUpdating) return
    const activating = selectedStation.status === StationStatus.INACTIVE
    setIsUpdating(true)
    setActionError(null)
    try {
      const updated = activating ? await activateStation(selectedStation.id) : await deactivateStation(selectedStation.id)
      setStations((current) => current.map((station) => station.id === updated.id ? updated : station))
      setSuccessMessage(`${updated.name} was ${activating ? 'activated' : 'deactivated'}.`)
      setSelectedStation(null)
    } catch (requestError) {
      setActionError(toApiClientError(requestError))
    } finally {
      setIsUpdating(false)
    }
  }

  const activating = selectedStation?.status === StationStatus.INACTIVE

  return (
    <>
      <PageHeader title="Microgrid Nodes" description="Manage station capacity, operational information, and availability." actions={<Link className="button button--primary" to="/stations/new">Create node</Link>} />
      <PageContent>
        {successMessage && <Alert variant="success" title="Microgrid node updated">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to update node">{actionError.message}</Alert>}
        {isLoading ? <div className="page-loading"><Spinner label="Loading microgrid nodes" /></div>
          : error ? <PageError message={error.message} onRetry={retry} />
            : stations.length === 0 ? <EmptyState title="No microgrid nodes found" description="Create a microgrid node to begin managing energy capacity and slots." action={<Link className="button button--primary" to="/stations/new">Create node</Link>} />
              : <StationsTable stations={stations} onStatusChange={(station) => { setActionError(null); setSelectedStation(station) }} />}
      </PageContent>
      <ConfirmationDialog
        open={selectedStation !== null}
        title={activating ? 'Activate microgrid node?' : 'Deactivate microgrid node?'}
        description={activating
          ? `Activate ${selectedStation?.name ?? 'this microgrid node'} for new operations?`
          : 'Deactivating this microgrid node prevents it from being used for new operations. The server will block deactivation when active reservations exist.'}
        confirmLabel={activating ? 'Activate' : 'Deactivate'}
        confirmVariant={activating ? 'primary' : 'danger'}
        errorMessage={actionError?.message}
        loading={isUpdating}
        onCancel={() => { setSelectedStation(null); setActionError(null) }}
        onConfirm={() => void handleStatusChange()}
      />
    </>
  )
}
