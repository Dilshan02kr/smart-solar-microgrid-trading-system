import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { UserRole } from '@/features/auth/types/authTypes'
import { getUser, updateUser } from '@/features/users/api/usersApi'
import { UserForm, type UserFormValues } from '@/features/users/components/UserForm'
import { useStations } from '@/features/users/hooks/useStations'
import type { UpdateWebUserRequest, WebUser } from '@/features/users/types/userTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function EditUserPage() {
  const { userId = '' } = useParams()
  const navigate = useNavigate()
  const [user, setUser] = useState<WebUser | null>(null)
  const [resolvedUserId, setResolvedUserId] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [loadError, setLoadError] = useState<ApiClientError | null>(null)
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)
  const { stations, isLoading: stationsLoading, error: stationsError, retry: retryStations } = useStations()

  const retry = useCallback(() => {
    setIsLoading(true)
    setNotFound(false)
    setLoadError(null)
    setRequestVersion((version) => version + 1)
    retryStations()
  }, [retryStations])

  useEffect(() => {
    const controller = new AbortController()
    getUser(userId, controller.signal)
      .then((loadedUser) => {
        setUser(loadedUser)
        setResolvedUserId(userId)
        setNotFound(false)
        setLoadError(null)
      })
      .catch((requestError: unknown) => {
        if (controller.signal.aborted) return
        const normalized = toApiClientError(requestError)
        setUser(null)
        setResolvedUserId(userId)
        if (normalized.status === 404) {
          setNotFound(true)
          setLoadError(null)
        } else {
          setNotFound(false)
          setLoadError(normalized)
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion, userId])

  async function handleSubmit(values: UserFormValues) {
    if (!user || isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    const request: UpdateWebUserRequest = {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      ...(user.role === UserRole.GRID_OPERATOR ? { assignedMicrogridNodeId: values.assignedMicrogridNodeId } : {}),
    }
    try {
      await updateUser(user.userId, request)
      navigate('/users', { replace: true, state: { success: 'The Web user was updated successfully.' } })
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      if (normalized.status === 404) setNotFound(true)
      else setApiError(normalized)
    } finally {
      setIsSubmitting(false)
    }
  }

  const needsStations = user?.role === UserRole.GRID_OPERATOR
  const loading = isLoading || resolvedUserId !== userId || (needsStations && stationsLoading)
  const error = loadError ?? (needsStations ? stationsError : null)

  return (
    <>
      <PageHeader title="Edit Web user" description="Update profile information and Grid Operator station assignment." />
      <PageContent>
        {loading ? (
          <div className="page-loading"><Spinner label="Loading Web user" /></div>
        ) : notFound ? (
          <EmptyState title="Web user not found" description="The requested Web user does not exist or is not managed through this area." action={<Link className="button button--primary" to="/users">Back to users</Link>} />
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : user ? (
          <UserForm
            mode="edit"
            initialValues={{ firstName: user.firstName, lastName: user.lastName, email: user.email, phone: user.phone, role: user.role, assignedMicrogridNodeId: user.assignedMicrogridNodeId ?? '' }}
            stations={stations}
            stationsLoading={stationsLoading}
            apiError={apiError}
            isSubmitting={isSubmitting}
            onSubmit={(values) => void handleSubmit(values)}
          />
        ) : null}
      </PageContent>
    </>
  )
}
