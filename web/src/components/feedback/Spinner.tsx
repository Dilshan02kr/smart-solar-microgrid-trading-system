export interface SpinnerProps {
  label?: string
  size?: 'small' | 'medium'
}

export function Spinner({ label = 'Loading', size = 'medium' }: SpinnerProps) {
  const spinner = <span className={`spinner spinner--${size}`} aria-hidden="true" />

  if (!label) return spinner

  return (
    <span className="loading-indicator" role="status">
      {spinner}
      <span>{label}</span>
    </span>
  )
}
