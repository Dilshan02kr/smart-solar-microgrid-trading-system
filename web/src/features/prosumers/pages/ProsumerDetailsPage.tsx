import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { AccountStatus } from '@/features/auth/types/authTypes'
import { activateProsumer, getProsumer, reactivateProsumer } from '@/features/prosumers/api/prosumersApi'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'
import { getAccountStatusBadgeVariant } from '@/utils/accountStatusPresentation'
import { formatDateTime } from '@/utils/formatDate'

type LifecycleAction = 'activate' | 'reactivate'

export function ProsumerDetailsPage() {
  const { prosumerId = '' } = useParams()
  const [prosumer, setProsumer] = useState<Prosumer | null>(null)
  const [resolvedProsumerId, setResolvedProsumerId] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [lifecycleAction, setLifecycleAction] = useState<LifecycleAction | null>(null)
  const [isUpdating, setIsUpdating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  function retry() {
    setIsLoading(true)
    setNotFound(false)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    getProsumer(prosumerId, controller.signal)
      .then((loadedProsumer) => {
        setProsumer(loadedProsumer)
        setResolvedProsumerId(prosumerId)
        setNotFound(false)
        setError(null)
      })
      .catch((requestError: unknown) => {
        if (controller.signal.aborted) return
        const normalized = toApiClientError(requestError)
        setProsumer(null)
        setResolvedProsumerId(prosumerId)
        if (normalized.status === 404) {
          setNotFound(true)
          setError(null)
        } else {
          setNotFound(false)
          setError(normalized)
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [prosumerId, requestVersion])

  const currentProsumer = resolvedProsumerId === prosumerId ? prosumer : null

  async function handleLifecycleChange() {
    if (!currentProsumer || !lifecycleAction || isUpdating) return
    setIsUpdating(true)
    setActionError(null)
    try {
      const updated = lifecycleAction === 'activate'
        ? await activateProsumer(currentProsumer.userId)
        : await reactivateProsumer(currentProsumer.userId)
      setProsumer(updated)
      setSuccessMessage(`${updated.firstName} ${updated.lastName} was ${lifecycleAction === 'activate' ? 'activated' : 'reactivated'}.`)
      setLifecycleAction(null)
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      if (normalized.status === 404) setNotFound(true)
      else setActionError(normalized)
    } finally {
      setIsUpdating(false)
    }
  }

  const action = currentProsumer?.accountStatus === AccountStatus.PENDING
    ? <Button onClick={() => setLifecycleAction('activate')}>Activate</Button>
    : currentProsumer?.accountStatus === AccountStatus.DEACTIVATED
      ? <Button onClick={() => setLifecycleAction('reactivate')}>Reactivate</Button>
      : undefined

  return (
    <>
      <PageHeader title="Prosumer details" description="Review safe account information and available lifecycle actions." actions={action} />
      <PageContent>
        {successMessage && <Alert variant="success" title="Account updated">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to update Prosumer">{actionError.message}</Alert>}
        {isLoading || resolvedProsumerId !== prosumerId ? (
          <div className="page-loading"><Spinner label="Loading Prosumer details" /></div>
        ) : notFound ? (
          <EmptyState title="Prosumer not found" description="The requested Prosumer account could not be found." action={<Link className="button button--primary" to="/prosumers">Back to Prosumers</Link>} />
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : currentProsumer ? (
          <section className="details-panel" aria-labelledby="prosumer-profile-title">
            <div className="details-panel__header">
              <div><h2 id="prosumer-profile-title">{currentProsumer.firstName} {currentProsumer.lastName}</h2><p>Prosumer account profile</p></div>
              <Badge variant={getAccountStatusBadgeVariant(currentProsumer.accountStatus)}>{currentProsumer.accountStatus}</Badge>
            </div>
            <dl className="details-grid">
              <div><dt>NIC</dt><dd>{currentProsumer.nic}</dd></div>
              <div><dt>Email</dt><dd>{currentProsumer.email}</dd></div>
              <div><dt>Phone</dt><dd>{currentProsumer.phone}</dd></div>
              <div><dt>Created</dt><dd>{formatDateTime(currentProsumer.createdAt)}</dd></div>
              <div><dt>Last updated</dt><dd>{formatDateTime(currentProsumer.updatedAt)}</dd></div>
            </dl>
            <div className="details-panel__footer"><Link to="/prosumers">Back to all Prosumers</Link></div>
          </section>
        ) : null}
      </PageContent>
      <ConfirmationDialog
        open={lifecycleAction !== null}
        title={lifecycleAction === 'reactivate' ? 'Reactivate Prosumer?' : 'Activate Prosumer?'}
        description={`${lifecycleAction === 'reactivate' ? 'Reactivate' : 'Activate'} ${currentProsumer?.firstName ?? 'this Prosumer'} ${currentProsumer?.lastName ?? ''}? The account will have access to authenticated Prosumer functions.`}
        confirmLabel={lifecycleAction === 'reactivate' ? 'Reactivate' : 'Activate'}
        confirmVariant="primary"
        loading={isUpdating}
        errorMessage={actionError?.message}
        onCancel={() => { setLifecycleAction(null); setActionError(null) }}
        onConfirm={() => void handleLifecycleChange()}
      />
    </>
  )
}
