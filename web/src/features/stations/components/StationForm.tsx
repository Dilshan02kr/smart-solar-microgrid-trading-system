import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { StationLocationPicker } from '@/features/stations/components/StationLocationPicker'
import type { CreateStationRequest, Station } from '@/features/stations/types/stationTypes'
import type { ApiClientError } from '@/services/api/apiError'
import { getApiFieldError } from '@/utils/apiFieldError'

interface StationFormValues {
  name: string
  locationName: string
  latitude: string
  longitude: string
  totalCapacityKw: string
  operationalSchedule: string
}

export interface StationFormProps {
  station?: Station
  apiError: ApiClientError | null
  isSubmitting: boolean
  cancelTo: string
  submitLabel: string
  onSubmit: (request: CreateStationRequest) => void
}

type FormErrors = Partial<Record<keyof StationFormValues, string>>

export function StationForm({ station, apiError, isSubmitting, cancelTo, submitLabel, onSubmit }: StationFormProps) {
  const [values, setValues] = useState<StationFormValues>({
    name: station?.name ?? '',
    locationName: station?.locationName ?? '',
    latitude: station ? String(station.latitude) : '',
    longitude: station ? String(station.longitude) : '',
    totalCapacityKw: station ? String(station.totalCapacityKw) : '',
    operationalSchedule: station?.operationalSchedule ?? '',
  })
  const [errors, setErrors] = useState<FormErrors>({})

  function updateField(field: keyof StationFormValues, value: string) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
  }

  function validate(): FormErrors {
    const next: FormErrors = {}
    const latitude = Number(values.latitude)
    const longitude = Number(values.longitude)
    const capacity = Number(values.totalCapacityKw)
    if (!values.name.trim()) next.name = 'Station name is required.'
    if (!values.locationName.trim()) next.locationName = 'Location name is required.'
    if (!values.latitude || !Number.isFinite(latitude) || latitude < -90 || latitude > 90) next.latitude = 'Latitude must be between -90 and 90.'
    if (!values.longitude || !Number.isFinite(longitude) || longitude < -180 || longitude > 180) next.longitude = 'Longitude must be between -180 and 180.'
    if (!values.totalCapacityKw || !Number.isFinite(capacity) || capacity <= 0) next.totalCapacityKw = 'Total capacity must be greater than 0 kW.'
    if (!values.operationalSchedule.trim()) next.operationalSchedule = 'Operational schedule is required.'
    return next
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) return
    const nextErrors = validate()
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length > 0) return
    onSubmit({
      name: values.name.trim(),
      locationName: values.locationName.trim(),
      latitude: Number(values.latitude),
      longitude: Number(values.longitude),
      totalCapacityKw: Number(values.totalCapacityKw),
      operationalSchedule: values.operationalSchedule.trim(),
    })
  }

  const fieldError = (field: keyof StationFormValues) => errors[field] ?? getApiFieldError(apiError, field)

  return (
    <Card>
      <form className="entity-form" onSubmit={handleSubmit} noValidate>
        {apiError && <Alert variant="error" title="Unable to save microgrid node">{apiError.message}</Alert>}
        <div className="form-grid">
          <Input label="Node name" value={values.name} onChange={(event) => updateField('name', event.target.value)} error={fieldError('name')} disabled={isSubmitting} required />
          <Input label="Location name" value={values.locationName} onChange={(event) => updateField('locationName', event.target.value)} error={fieldError('locationName')} disabled={isSubmitting} required />
          <div className="form-grid__full">
            <StationLocationPicker
              latitude={values.latitude}
              longitude={values.longitude}
              disabled={isSubmitting}
              onSelect={(latitude, longitude) => {
                updateField('latitude', latitude.toFixed(6))
                updateField('longitude', longitude.toFixed(6))
              }}
            />
          </div>
          <Input label="Latitude" type="number" step="any" min="-90" max="90" value={values.latitude} onChange={(event) => updateField('latitude', event.target.value)} error={fieldError('latitude')} helperText="Decimal degrees from -90 to 90." disabled={isSubmitting} required />
          <Input label="Longitude" type="number" step="any" min="-180" max="180" value={values.longitude} onChange={(event) => updateField('longitude', event.target.value)} error={fieldError('longitude')} helperText="Decimal degrees from -180 to 180." disabled={isSubmitting} required />
          <Input label="Total capacity (kW)" type="number" step="any" min="0" value={values.totalCapacityKw} onChange={(event) => updateField('totalCapacityKw', event.target.value)} error={fieldError('totalCapacityKw')} disabled={isSubmitting} required />
          <Input label="Operational schedule" value={values.operationalSchedule} onChange={(event) => updateField('operationalSchedule', event.target.value)} error={fieldError('operationalSchedule')} placeholder="Example: Daily, 06:00–18:00" helperText="Use the operating information maintained for this node." disabled={isSubmitting} required />
        </div>
        <div className="form-actions">
          <Link className="button button--outline" to={cancelTo}>Cancel</Link>
          <Button type="submit" loading={isSubmitting} disabled={isSubmitting}>{submitLabel}</Button>
        </div>
      </form>
    </Card>
  )
}
