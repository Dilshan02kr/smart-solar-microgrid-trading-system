import { useEffect, useMemo, useState } from 'react'
import { Button } from '@/components/ui/Button'
import { FilterBar } from '@/components/ui/FilterBar'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { AccountStatus } from '@/features/auth/types/authTypes'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { getProsumers } from '@/features/prosumers/api/prosumersApi'
import { ProsumerSubnavigation } from '@/features/prosumers/components/ProsumerSubnavigation'
import { ProsumersTable } from '@/features/prosumers/components/ProsumersTable'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function ProsumersPage() {
  const [prosumers, setProsumers] = useState<Prosumer[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')

  const filteredProsumers = useMemo(() => {
    const query = search.trim().toLowerCase()
    return prosumers.filter((prosumer) => (!statusFilter || prosumer.accountStatus === statusFilter) && (!query || `${prosumer.firstName} ${prosumer.lastName} ${prosumer.email} ${prosumer.nic}`.toLowerCase().includes(query)))
  }, [prosumers, search, statusFilter])

  function clearFilters() { setSearch(''); setStatusFilter('') }

  function retry() {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    getProsumers(controller.signal)
      .then(setProsumers)
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  return (
    <>
      <PageHeader title="Prosumers" description="Review registered Prosumer accounts and their current lifecycle status." />
      <PageContent>
        <ProsumerSubnavigation />
        {!isLoading && !error && prosumers.length > 0 && <FilterBar onClear={clearFilters} resultCount={filteredProsumers.length} title="Filter Prosumers">
          <Input label="Search" type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Name, email, or NIC" />
          <Select label="Account status" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} options={[{ label: 'All statuses', value: '' }, { label: 'Pending', value: AccountStatus.PENDING }, { label: 'Active', value: AccountStatus.ACTIVE }, { label: 'Deactivated', value: AccountStatus.DEACTIVATED }]} />
        </FilterBar>}
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading Prosumers" /></div>
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : prosumers.length === 0 ? (
          <EmptyState title="No Prosumers found" description="Registered Prosumer accounts will appear here." />
        ) : filteredProsumers.length === 0 ? (
          <EmptyState title="No matching Prosumers" description="Try changing or clearing the current filters." action={<Button variant="outline" onClick={clearFilters}>Clear filters</Button>} />
        ) : (
          <ProsumersTable prosumers={filteredProsumers} />
        )}
      </PageContent>
    </>
  )
}
