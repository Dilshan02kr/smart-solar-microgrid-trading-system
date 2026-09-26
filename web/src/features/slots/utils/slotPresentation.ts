export function toDateInputValue(value: string): string {
  return value.slice(0, 10)
}

export function toTimeInputValue(value: string): string {
  return value.slice(0, 5)
}

export function toApiTime(value: string): string {
  return value.length === 5 ? `${value}:00` : value
}

export function formatSlotDate(value: string): string {
  const [year, month, day] = value.slice(0, 10).split('-').map(Number)
  if (!year || !month || !day) return 'Not available'
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(year, month - 1, day))
}

export function formatSlotTime(value: string): string {
  return value.slice(0, 5)
}
