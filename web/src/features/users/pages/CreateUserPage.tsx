import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { UserRole } from '@/features/auth/types/authTypes'
import { createUser } from '@/features/users/api/usersApi'
import { UserForm, type UserFormValues } from '@/features/users/components/UserForm'
import { useStations } from '@/features/users/hooks/useStations'
import type { CreateWebUserRequest } from '@/features/users/types/userTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function CreateUserPage() {
  const navigate = useNavigate()
  const { stations, isLoading: stationsLoading, error: stationsError, retry } = useStations()
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(values: UserFormValues) {
    if (isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    const request: CreateWebUserRequest = {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      password: values.password,
      confirmPassword: values.confirmPassword,
      role: values.role,
      ...(values.role === UserRole.GRID_OPERATOR ? { assignedMicrogridNodeId: values.assignedMicrogridNodeId } : {}),
    }
    try {
      await createUser(request)
      navigate('/users', { replace: true, state: { success: 'The Web user was created successfully.' } })
    } catch (requestError) {
      setApiError(toApiClientError(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader title="Create Web user" description="Create an active Backoffice account or an assigned Grid Operator account." />
      <PageContent>
        {stationsError && (
          <div className="inline-retry">
            <Alert variant="warning" title="Microgrid nodes unavailable">{stationsError.message} Backoffice accounts can still be created.</Alert>
            <Button variant="outline" onClick={retry}>Retry lookup</Button>
          </div>
        )}
        <UserForm mode="create" stations={stations} stationsLoading={stationsLoading} apiError={apiError} isSubmitting={isSubmitting} onSubmit={(values) => void handleSubmit(values)} />
      </PageContent>
    </>
  )
}
