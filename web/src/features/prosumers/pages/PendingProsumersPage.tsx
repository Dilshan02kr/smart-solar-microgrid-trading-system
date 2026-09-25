import { useEffect, useState } from 'react'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { activateProsumer, getPendingProsumers } from '@/features/prosumers/api/prosumersApi'
import { ProsumerSubnavigation } from '@/features/prosumers/components/ProsumerSubnavigation'
import { ProsumersTable } from '@/features/prosumers/components/ProsumersTable'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function PendingProsumersPage() {
  const [prosumers, setProsumers] = useState<Prosumer[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [selectedProsumer, setSelectedProsumer] = useState<Prosumer | null>(null)
  const [isActivating, setIsActivating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  function retry() {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    getPendingProsumers(controller.signal)
      .then(setProsumers)
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  async function handleActivate() {
    if (!selectedProsumer || isActivating) return
    setIsActivating(true)
    setActionError(null)
    try {
      const activated = await activateProsumer(selectedProsumer.userId)
      setProsumers((current) => current.filter((prosumer) => prosumer.userId !== activated.userId))
      setSuccessMessage(`${activated.firstName} ${activated.lastName} was activated.`)
      setSelectedProsumer(null)
    } catch (requestError) {
      setActionError(toApiClientError(requestError))
    } finally {
      setIsActivating(false)
    }
  }

  return (
    <>
      <PageHeader title="Pending Prosumer registrations" description="Activate verified Prosumer accounts that are waiting for access." />
      <PageContent>
        <ProsumerSubnavigation />
        {successMessage && <Alert variant="success" title="Prosumer activated">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to activate Prosumer">{actionError.message}</Alert>}
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading pending Prosumers" /></div>
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : prosumers.length === 0 ? (
          <EmptyState title="No pending registrations" description="There are currently no Prosumer accounts waiting for activation." />
        ) : (
          <ProsumersTable prosumers={prosumers} onActivate={(prosumer) => { setActionError(null); setSelectedProsumer(prosumer) }} />
        )}
      </PageContent>
      <ConfirmationDialog
        open={selectedProsumer !== null}
        title="Activate Prosumer?"
        description={`Activate ${selectedProsumer?.firstName ?? 'this Prosumer'} ${selectedProsumer?.lastName ?? ''}? The account will be able to use authenticated Prosumer functions.`}
        confirmLabel="Activate"
        confirmVariant="primary"
        loading={isActivating}
        errorMessage={actionError?.message}
        onCancel={() => { setSelectedProsumer(null); setActionError(null) }}
        onConfirm={() => void handleActivate()}
      />
    </>
  )
}
