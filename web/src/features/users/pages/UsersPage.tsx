import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { FilterBar } from '@/components/ui/FilterBar'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { AccountStatus, UserRole } from '@/features/auth/types/authTypes'
import { getRoleLabel } from '@/features/auth/utils/rolePresentation'
import { deactivateUser, getUsers } from '@/features/users/api/usersApi'
import { useStations } from '@/features/users/hooks/useStations'
import type { WebUser } from '@/features/users/types/userTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'
import { getAccountStatusBadgeVariant } from '@/utils/accountStatusPresentation'
import { formatDateTime } from '@/utils/formatDate'

interface UsersLocationState {
  success?: string
}

export function UsersPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const [users, setUsers] = useState<WebUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState((location.state as UsersLocationState | null)?.success ?? '')
  const [selectedUser, setSelectedUser] = useState<WebUser | null>(null)
  const [isDeactivating, setIsDeactivating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)
  const [activeTab, setActiveTab] = useState<'backoffice' | 'operators'>('backoffice')
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [stationFilter, setStationFilter] = useState('')
  const { stations, error: stationsError, retry: retryStations } = useStations()

  const retryUsers = useCallback(() => {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }, [])

  useEffect(() => {
    if ((location.state as UsersLocationState | null)?.success) {
      void navigate(location.pathname, { replace: true, state: null })
    }
  }, [location.pathname, location.state, navigate])

  useEffect(() => {
    const controller = new AbortController()
    getUsers(controller.signal)
      .then(setUsers)
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  const stationNames = useMemo(
    () => new Map(stations.map((station) => [station.id, station.name])),
    [stations],
  )

  const filteredUsers = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase()
    const role = activeTab === 'backoffice' ? UserRole.BACKOFFICE : UserRole.GRID_OPERATOR
    return users
      .filter((user) => user.role === role)
      .filter((user) => !statusFilter || user.accountStatus === statusFilter)
      .filter((user) => activeTab === 'backoffice' || !stationFilter || (stationFilter === 'unassigned' ? !user.assignedMicrogridNodeId : user.assignedMicrogridNodeId === stationFilter))
      .filter((user) => !normalizedSearch || `${user.firstName} ${user.lastName} ${user.email} ${user.phone}`.toLowerCase().includes(normalizedSearch))
      .sort((a, b) => a.firstName.localeCompare(b.firstName))
  }, [activeTab, search, stationFilter, statusFilter, users])

  function clearFilters() { setSearch(''); setStatusFilter(''); setStationFilter('') }

  async function handleDeactivate() {
    if (!selectedUser || isDeactivating) return
    setIsDeactivating(true)
    setActionError(null)
    try {
      const updatedUser = await deactivateUser(selectedUser.userId)
      setUsers((current) => current.map((user) => user.userId === updatedUser.userId ? updatedUser : user))
      setSuccessMessage(`${updatedUser.firstName} ${updatedUser.lastName} was deactivated.`)
      setSelectedUser(null)
    } catch (requestError) {
      setActionError(toApiClientError(requestError))
    } finally {
      setIsDeactivating(false)
    }
  }

  return (
    <>
      <PageHeader
        title="Web users"
        description="Manage Backoffice and Grid Operator accounts and station assignments."
        actions={<Link className="button button--primary" to="/users/new">Create user</Link>}
      />
      <PageContent>
        {successMessage && <Alert variant="success" title="User management updated">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to deactivate user">{actionError.message}</Alert>}
        {stationsError && !isLoading && !error && (
          <div className="inline-retry">
            <Alert variant="warning" title="Microgrid node names unavailable">Assignments are shown using their node IDs. {stationsError.message}</Alert>
            <Button variant="outline" onClick={retryStations}>Retry lookup</Button>
          </div>
        )}
        {!isLoading && !error && users.length > 0 && <>
          <div className="segmented-tabs" role="tablist" aria-label="User type">
            <button role="tab" aria-selected={activeTab === 'backoffice'} onClick={() => { setActiveTab('backoffice'); clearFilters() }}>Backoffice Users ({users.filter((user) => user.role === UserRole.BACKOFFICE).length})</button>
            <button role="tab" aria-selected={activeTab === 'operators'} onClick={() => { setActiveTab('operators'); clearFilters() }}>Grid Operators ({users.filter((user) => user.role === UserRole.GRID_OPERATOR).length})</button>
          </div>
          <FilterBar onClear={clearFilters} resultCount={filteredUsers.length} title={`Filter ${activeTab === 'backoffice' ? 'Backoffice users' : 'Grid Operators'}`}>
            <Input label="Search" type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Name, email, or phone" />
            <Select label="Account status" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} options={[{ label: 'All statuses', value: '' }, { label: 'Active', value: AccountStatus.ACTIVE }, { label: 'Deactivated', value: AccountStatus.DEACTIVATED }]} />
            {activeTab === 'operators' && <Select label="Assigned node" value={stationFilter} onChange={(event) => setStationFilter(event.target.value)} options={[{ label: 'All nodes', value: '' }, { label: 'Not assigned', value: 'unassigned' }, ...stations.map((station) => ({ label: station.name, value: station.id }))]} />}
          </FilterBar>
        </>}
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading Web users" /></div>
        ) : error ? (
          <PageError message={error.message} onRetry={retryUsers} />
        ) : users.length === 0 ? (
          <EmptyState title="No Web users found" description="Create a Backoffice or Grid Operator account to get started." action={<Link className="button button--primary" to="/users/new">Create user</Link>} />
        ) : filteredUsers.length === 0 ? (
          <EmptyState title="No matching users" description="Try changing or clearing the current search filters." action={<Button variant="outline" onClick={clearFilters}>Clear filters</Button>} />
        ) : (
          <DataTable caption={activeTab === 'backoffice' ? 'Backoffice users' : 'Grid Operators'}>
            <thead>
              <tr><th scope="col">Name</th><th scope="col">Email</th><th scope="col">Phone</th><th scope="col">Role</th>{activeTab === 'operators' && <th scope="col">Assigned Microgrid Node</th>}<th scope="col">Status</th><th scope="col">Created</th><th scope="col">Actions</th></tr>
            </thead>
            <tbody>
              {filteredUsers.map((user) => (
                <tr key={user.userId}>
                  <td><strong>{user.firstName} {user.lastName}</strong></td>
                  <td>{user.email}</td>
                  <td>{user.phone}</td>
                  <td>{getRoleLabel(user.role)}</td>
                  {activeTab === 'operators' && <td>{user.assignedMicrogridNodeId ? stationNames.get(user.assignedMicrogridNodeId) ?? user.assignedMicrogridNodeId : <Badge variant="warning">Not assigned</Badge>}</td>}
                  <td><Badge variant={getAccountStatusBadgeVariant(user.accountStatus)}>{user.accountStatus}</Badge></td>
                  <td>{formatDateTime(user.createdAt)}</td>
                  <td>
                    <div className="table-actions">
                      <Link className="button button--ghost" to={`/users/${user.userId}/edit`}>Edit</Link>
                      {user.accountStatus === AccountStatus.ACTIVE && <Button variant="ghost" onClick={() => { setActionError(null); setSelectedUser(user) }}>Deactivate</Button>}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </DataTable>
        )}
      </PageContent>
      <ConfirmationDialog
        open={selectedUser !== null}
        title="Deactivate account?"
        description={`Deactivate ${selectedUser?.firstName ?? 'this'} ${selectedUser?.lastName ?? 'account'}? The user will immediately lose access to protected system functions.`}
        confirmLabel="Deactivate"
        confirmVariant="danger"
        loading={isDeactivating}
        errorMessage={actionError?.message}
        onCancel={() => { setSelectedUser(null); setActionError(null) }}
        onConfirm={() => void handleDeactivate()}
      />
    </>
  )
}
