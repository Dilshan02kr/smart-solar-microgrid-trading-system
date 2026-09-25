import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import type { EnergyBookingSlot, UpdateEnergyBookingSlotRequest } from '@/features/slots/types/slotTypes'
import { toApiTime, toDateInputValue, toTimeInputValue } from '@/features/slots/utils/slotPresentation'
import type { ApiClientError } from '@/services/api/apiError'
import { getApiFieldError } from '@/utils/apiFieldError'

interface SlotFormValues {
  date: string
  startTime: string
  endTime: string
  capacityKw: string
}

export interface SlotFormProps {
  slot?: EnergyBookingSlot
  stationCapacityKw: number
  apiError: ApiClientError | null
  isSubmitting: boolean
  cancelTo: string
  submitLabel: string
  onSubmit: (request: UpdateEnergyBookingSlotRequest) => void
}

type FormErrors = Partial<Record<keyof SlotFormValues, string>>

export function SlotForm({ slot, stationCapacityKw, apiError, isSubmitting, cancelTo, submitLabel, onSubmit }: SlotFormProps) {
  const [values, setValues] = useState<SlotFormValues>({
    date: slot ? toDateInputValue(slot.date) : '',
    startTime: slot ? toTimeInputValue(slot.startTime) : '',
    endTime: slot ? toTimeInputValue(slot.endTime) : '',
    capacityKw: slot ? String(slot.capacityKw) : '',
  })
  const [errors, setErrors] = useState<FormErrors>({})

  function updateField(field: keyof SlotFormValues, value: string) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
  }

  function validate(): FormErrors {
    const next: FormErrors = {}
    const capacity = Number(values.capacityKw)
    if (!values.date) next.date = 'Date is required.'
    if (!values.startTime) next.startTime = 'Start time is required.'
    if (!values.endTime) next.endTime = 'End time is required.'
    else if (values.startTime && values.startTime >= values.endTime) next.endTime = 'End time must be after start time.'
    if (!values.capacityKw || !Number.isFinite(capacity) || capacity <= 0) next.capacityKw = 'Capacity must be greater than 0 kW.'
    return next
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) return
    const nextErrors = validate()
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length > 0) return
    onSubmit({
      date: values.date,
      startTime: toApiTime(values.startTime),
      endTime: toApiTime(values.endTime),
      capacityKw: Number(values.capacityKw),
    })
  }

  const fieldError = (field: keyof SlotFormValues) => errors[field] ?? getApiFieldError(apiError, field)

  return (
    <Card>
      <form className="entity-form" onSubmit={handleSubmit} noValidate>
        {apiError && <Alert variant="error" title="Unable to save energy slot">{apiError.message}</Alert>}
        <div className="form-grid">
          <Input label="Date" type="date" value={values.date} onChange={(event) => updateField('date', event.target.value)} error={fieldError('date')} disabled={isSubmitting} required />
          <Input label="Capacity (kW)" type="number" step="any" min="0" value={values.capacityKw} onChange={(event) => updateField('capacityKw', event.target.value)} error={fieldError('capacityKw')} helperText={`Node capacity: ${stationCapacityKw} kW. The server validates the slot limit.`} disabled={isSubmitting} required />
          <Input label="Start time" type="time" value={values.startTime} onChange={(event) => updateField('startTime', event.target.value)} error={fieldError('startTime')} disabled={isSubmitting} required />
          <Input label="End time" type="time" value={values.endTime} onChange={(event) => updateField('endTime', event.target.value)} error={fieldError('endTime')} disabled={isSubmitting} required />
        </div>
        <div className="form-actions">
          <Link className="button button--outline" to={cancelTo}>Cancel</Link>
          <Button type="submit" loading={isSubmitting} disabled={isSubmitting}>{submitLabel}</Button>
        </div>
      </form>
    </Card>
  )
}
