import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Button } from '@/components/ui/Button'
import { FilterBar } from '@/components/ui/FilterBar'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { UserRole } from '@/features/auth/types/authTypes'
import { activateStation, deactivateStation, getStations } from '@/features/stations/api/stationsApi'
import { StationsTable } from '@/features/stations/components/StationsTable'
import { StationStatus, type Station } from '@/features/stations/types/stationTypes'
import { getUsers } from '@/features/users/api/usersApi'
import type { WebUser } from '@/features/users/types/userTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function StationsPage() {
  const [stations, setStations] = useState<Station[]>([])
  const [operators, setOperators] = useState<WebUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [selectedStation, setSelectedStation] = useState<Station | null>(null)
  const [isUpdating, setIsUpdating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [operatorFilter, setOperatorFilter] = useState('')

  const operatorNames = useMemo(() => new Map(operators.filter((user) => user.assignedMicrogridNodeId).map((user) => [user.assignedMicrogridNodeId as string, `${user.firstName} ${user.lastName}`])), [operators])

  const filteredStations = useMemo(() => {
    const query = search.trim().toLowerCase()
    return stations.filter((station) => (!statusFilter || station.status === statusFilter) && (!operatorFilter || (operatorFilter === 'unassigned' ? !operatorNames.has(station.id) : operators.some((operator) => operator.userId === operatorFilter && operator.assignedMicrogridNodeId === station.id))) && (!query || `${station.name} ${station.locationName} ${station.operationalSchedule} ${operatorNames.get(station.id) ?? ''}`.toLowerCase().includes(query)))
  }, [operatorFilter, operatorNames, operators, search, stations, statusFilter])

  function clearFilters() { setSearch(''); setStatusFilter(''); setOperatorFilter('') }

  const retry = useCallback(() => {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([getStations(controller.signal), getUsers(controller.signal)])
      .then(([stationResults, userResults]) => { setStations(stationResults); setOperators(userResults.filter((user) => user.role === UserRole.GRID_OPERATOR)) })
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
        {!isLoading && !error && stations.length > 0 && <FilterBar onClear={clearFilters} resultCount={filteredStations.length} title="Filter microgrid nodes">
          <Input label="Search" type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Name, location, or schedule" />
          <Select label="Node status" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} options={[{ label: 'All statuses', value: '' }, { label: 'Active', value: StationStatus.ACTIVE }, { label: 'Inactive', value: StationStatus.INACTIVE }]} />
          <Select label="Grid Operator" value={operatorFilter} onChange={(event) => setOperatorFilter(event.target.value)} options={[{ label: 'All operators', value: '' }, { label: 'Not assigned', value: 'unassigned' }, ...operators.map((operator) => ({ label: `${operator.firstName} ${operator.lastName}`, value: operator.userId }))]} />
        </FilterBar>}
        {isLoading ? <div className="page-loading"><Spinner label="Loading microgrid nodes" /></div>
          : error ? <PageError message={error.message} onRetry={retry} />
            : stations.length === 0 ? <EmptyState title="No microgrid nodes found" description="Create a microgrid node to begin managing energy capacity and slots." action={<Link className="button button--primary" to="/stations/new">Create node</Link>} />
              : filteredStations.length === 0 ? <EmptyState title="No matching microgrid nodes" description="Try changing or clearing the current filters." action={<Button variant="outline" onClick={clearFilters}>Clear filters</Button>} />
                : <StationsTable stations={filteredStations} operatorNames={operatorNames} onStatusChange={(station) => { setActionError(null); setSelectedStation(station) }} />}
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
