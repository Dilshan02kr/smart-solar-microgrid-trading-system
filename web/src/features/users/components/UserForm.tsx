import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { UserRole } from '@/features/auth/types/authTypes'
import { getRoleLabel } from '@/features/auth/utils/rolePresentation'
import type { StationResponse, WebUserRole } from '@/features/users/types/userTypes'
import { getApiFieldError } from '@/features/users/utils/userFormErrors'
import type { ApiClientError } from '@/services/api/apiError'

export interface UserFormValues {
  firstName: string
  lastName: string
  email: string
  phone: string
  password: string
  confirmPassword: string
  role: WebUserRole
  assignedMicrogridNodeId: string
}

export interface UserFormProps {
  mode: 'create' | 'edit'
  initialValues?: Partial<UserFormValues>
  stations: StationResponse[]
  stationsLoading: boolean
  apiError: ApiClientError | null
  isSubmitting: boolean
  onSubmit: (values: UserFormValues) => void
}

type FormErrors = Partial<Record<keyof UserFormValues, string>>

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function UserForm({ mode, initialValues, stations, stationsLoading, apiError, isSubmitting, onSubmit }: UserFormProps) {
  const [values, setValues] = useState<UserFormValues>({
    firstName: initialValues?.firstName ?? '',
    lastName: initialValues?.lastName ?? '',
    email: initialValues?.email ?? '',
    phone: initialValues?.phone ?? '',
    password: '',
    confirmPassword: '',
    role: initialValues?.role ?? UserRole.BACKOFFICE,
    assignedMicrogridNodeId: initialValues?.assignedMicrogridNodeId ?? '',
  })
  const [errors, setErrors] = useState<FormErrors>({})

  function updateField<K extends keyof UserFormValues>(field: K, value: UserFormValues[K]) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
  }

  function validate(): FormErrors {
    const next: FormErrors = {}
    if (!values.firstName.trim()) next.firstName = 'First name is required.'
    if (!values.lastName.trim()) next.lastName = 'Last name is required.'
    if (!emailPattern.test(values.email.trim())) next.email = 'Enter a valid email address.'
    if (!values.phone.trim()) next.phone = 'Phone is required.'
    if (mode === 'create') {
      if (!values.password) next.password = 'Password is required.'
      if (!values.confirmPassword) next.confirmPassword = 'Confirm the password.'
      else if (values.password !== values.confirmPassword) next.confirmPassword = 'Passwords must match.'
    }
    if (values.role === UserRole.GRID_OPERATOR && !values.assignedMicrogridNodeId) {
      next.assignedMicrogridNodeId = 'Select a microgrid node for this Grid Operator.'
    }
    return next
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) return
    const nextErrors = validate()
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length === 0) onSubmit(values)
  }

  const fieldError = (field: keyof UserFormValues) => errors[field] ?? getApiFieldError(apiError, field)
  const stationOptions = stations.map((station) => ({
    value: station.id,
    label: `${station.name} — ${station.locationName}`,
  }))

  return (
    <Card>
      <form className="entity-form" onSubmit={handleSubmit} noValidate>
        {apiError && <Alert variant="error" title="Unable to save user">{apiError.message}</Alert>}
        <div className="form-grid">
          <Input label="First name" value={values.firstName} onChange={(event) => updateField('firstName', event.target.value)} error={fieldError('firstName')} disabled={isSubmitting} required />
          <Input label="Last name" value={values.lastName} onChange={(event) => updateField('lastName', event.target.value)} error={fieldError('lastName')} disabled={isSubmitting} required />
          <Input label="Email" type="email" autoComplete="email" value={values.email} onChange={(event) => updateField('email', event.target.value)} error={fieldError('email')} disabled={isSubmitting} required />
          <Input label="Phone" type="tel" autoComplete="tel" value={values.phone} onChange={(event) => updateField('phone', event.target.value)} error={fieldError('phone')} disabled={isSubmitting} required />
          {mode === 'create' ? (
            <>
              <Input label="Password" type="password" autoComplete="new-password" value={values.password} onChange={(event) => updateField('password', event.target.value)} error={fieldError('password')} disabled={isSubmitting} required />
              <Input label="Confirm password" type="password" autoComplete="new-password" value={values.confirmPassword} onChange={(event) => updateField('confirmPassword', event.target.value)} error={fieldError('confirmPassword')} disabled={isSubmitting} required />
              <Select
                label="Role"
                value={values.role}
                onChange={(event) => {
                  const role = event.target.value as WebUserRole
                  setValues((current) => ({ ...current, role, assignedMicrogridNodeId: role === UserRole.BACKOFFICE ? '' : current.assignedMicrogridNodeId }))
                }}
                options={[
                  { value: UserRole.BACKOFFICE, label: getRoleLabel(UserRole.BACKOFFICE) },
                  { value: UserRole.GRID_OPERATOR, label: getRoleLabel(UserRole.GRID_OPERATOR) },
                ]}
                disabled={isSubmitting}
                required
              />
            </>
          ) : (
            <Input label="Role" value={getRoleLabel(values.role)} disabled />
          )}
          {values.role === UserRole.GRID_OPERATOR && (
            <Select
              label="Assigned Microgrid Node"
              value={values.assignedMicrogridNodeId}
              onChange={(event) => updateField('assignedMicrogridNodeId', event.target.value)}
              options={stationOptions}
              placeholder={stationsLoading ? 'Loading microgrid nodes…' : 'Select a microgrid node'}
              error={fieldError('assignedMicrogridNodeId') ?? getApiFieldError(apiError, 'assignedMicrogridNodeId')}
              disabled={isSubmitting || stationsLoading}
              required
            />
          )}
        </div>
        <div className="form-actions">
          <Link className="button button--outline" to="/users">Cancel</Link>
          <Button type="submit" loading={isSubmitting} disabled={isSubmitting || (values.role === UserRole.GRID_OPERATOR && stationsLoading)}>
            {mode === 'create' ? 'Create user' : 'Save changes'}
          </Button>
        </div>
      </form>
    </Card>
  )
}
