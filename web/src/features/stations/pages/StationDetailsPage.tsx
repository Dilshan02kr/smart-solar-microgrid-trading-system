import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { activateStation, deactivateStation } from '@/features/stations/api/stationsApi'
import { useStation } from '@/features/stations/hooks/useStation'
import { StationStatus } from '@/features/stations/types/stationTypes'
import { formatCapacity, getStationStatusBadgeVariant } from '@/features/stations/utils/stationPresentation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'
import { formatDateTime } from '@/utils/formatDate'

interface StationLocationState { success?: string }

export function StationDetailsPage() {
  const { stationId = '' } = useParams()
  const location = useLocation()
  const navigate = useNavigate()
  const { station, setStation, isLoading, notFound, error, retry } = useStation(stationId)
  const [successMessage, setSuccessMessage] = useState((location.state as StationLocationState | null)?.success ?? '')
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [isUpdating, setIsUpdating] = useState(false)

  useEffect(() => {
    if ((location.state as StationLocationState | null)?.success) void navigate(location.pathname, { replace: true, state: null })
  }, [location.pathname, location.state, navigate])

  async function handleStatusChange() {
    if (!station || isUpdating) return
    const activating = station.status === StationStatus.INACTIVE
    setIsUpdating(true)
    setActionError(null)
    try {
      const updated = activating ? await activateStation(station.id) : await deactivateStation(station.id)
      setStation(updated)
      setSuccessMessage(`${updated.name} was ${activating ? 'activated' : 'deactivated'}.`)
      setConfirmOpen(false)
    } catch (requestError) {
      setActionError(toApiClientError(requestError))
    } finally {
      setIsUpdating(false)
    }
  }

  const activating = station?.status === StationStatus.INACTIVE
  const actions = station ? <>
    <Link className="button button--outline" to={`/stations/${station.id}/edit`}>Edit</Link>
    <Link className="button button--secondary" to={`/stations/${station.id}/slots`}>Manage Energy Slots</Link>
    <Button variant={activating ? 'primary' : 'danger'} onClick={() => { setActionError(null); setConfirmOpen(true) }}>{activating ? 'Activate' : 'Deactivate'}</Button>
  </> : undefined

  return (
    <>
      <PageHeader title="Microgrid node details" description="Review station configuration and manage its operational state." actions={actions} />
      <PageContent>
        {successMessage && <Alert variant="success" title="Microgrid node updated">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to update node">{actionError.message}</Alert>}
        {isLoading ? <div className="page-loading"><Spinner label="Loading microgrid node" /></div>
          : notFound ? <EmptyState title="Microgrid node not found" description="The requested node could not be found." action={<Link className="button button--primary" to="/stations">Back to Microgrid Nodes</Link>} />
            : error ? <PageError message={error.message} onRetry={retry} />
              : station ? <section className="details-panel" aria-labelledby="station-name">
                <div className="details-panel__header"><div><h2 id="station-name">{station.name}</h2><p>{station.locationName}</p></div><Badge variant={getStationStatusBadgeVariant(station.status)}>{station.status}</Badge></div>
                <dl className="details-grid">
                  <div><dt>Total capacity</dt><dd>{formatCapacity(station.totalCapacityKw)}</dd></div>
                  <div><dt>Operational schedule</dt><dd>{station.operationalSchedule}</dd></div>
                  <div><dt>Latitude</dt><dd>{station.latitude}</dd></div>
                  <div><dt>Longitude</dt><dd>{station.longitude}</dd></div>
                  <div><dt>Created</dt><dd>{formatDateTime(station.createdAt)}</dd></div>
                  <div><dt>Last updated</dt><dd>{formatDateTime(station.updatedAt)}</dd></div>
                </dl>
                <div className="details-panel__footer"><Link to="/stations">Back to Microgrid Nodes</Link></div>
              </section> : null}
      </PageContent>
      <ConfirmationDialog
        open={confirmOpen}
        title={activating ? 'Activate microgrid node?' : 'Deactivate microgrid node?'}
        description={activating ? `Activate ${station?.name ?? 'this microgrid node'} for new operations?` : 'Deactivating this microgrid node prevents it from being used for new operations. The server will block deactivation when active reservations exist.'}
        confirmLabel={activating ? 'Activate' : 'Deactivate'}
        confirmVariant={activating ? 'primary' : 'danger'}
        errorMessage={actionError?.message}
        loading={isUpdating}
        onCancel={() => { setConfirmOpen(false); setActionError(null) }}
        onConfirm={() => void handleStatusChange()}
      />
    </>
  )
}
