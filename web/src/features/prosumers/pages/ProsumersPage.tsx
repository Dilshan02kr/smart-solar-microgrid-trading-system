import { useEffect, useState } from 'react'
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
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading Prosumers" /></div>
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : prosumers.length === 0 ? (
          <EmptyState title="No Prosumers found" description="Registered Prosumer accounts will appear here." />
        ) : (
          <ProsumersTable prosumers={prosumers} />
        )}
      </PageContent>
    </>
  )
}
