import { useId, type SelectHTMLAttributes } from 'react'

export interface SelectOption {
  label: string
  value: string
  disabled?: boolean
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  options: SelectOption[]
  placeholder?: string
  error?: string
}

export function Select({ label, options, placeholder, error, required, id, className = '', value, defaultValue, ...props }: SelectProps) {
  const generatedId = useId()
  const selectId = id ?? generatedId
  const errorId = `${selectId}-error`

  return (
    <div className="field">
      <label className="field__label" htmlFor={selectId}>
        {label} {required && <span className="field__required" aria-hidden="true">*</span>}
      </label>
      <select
        id={selectId}
        className={['field__control', className].filter(Boolean).join(' ')}
        required={required}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? errorId : undefined}
        value={value}
        defaultValue={value === undefined ? defaultValue ?? (placeholder ? '' : undefined) : undefined}
        {...props}
      >
        {placeholder && <option value="" disabled>{placeholder}</option>}
        {options.map((option) => (
          <option key={option.value} value={option.value} disabled={option.disabled}>{option.label}</option>
        ))}
      </select>
      {error && <p id={errorId} className="field__message field__message--error" role="alert">{error}</p>}
    </div>
  )
}
