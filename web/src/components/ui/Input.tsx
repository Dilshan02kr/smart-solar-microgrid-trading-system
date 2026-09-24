import { useId, type InputHTMLAttributes } from 'react'

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
  helperText?: string
}

export function Input({ label, error, helperText, required, id, className = '', ...props }: InputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const messageId = `${inputId}-message`

  return (
    <div className="field">
      <label className="field__label" htmlFor={inputId}>
        {label} {required && <span className="field__required" aria-hidden="true">*</span>}
      </label>
      <input
        id={inputId}
        className={['field__control', className].filter(Boolean).join(' ')}
        required={required}
        aria-invalid={Boolean(error)}
        aria-describedby={error || helperText ? messageId : undefined}
        {...props}
      />
      {(error || helperText) && (
        <p id={messageId} className={`field__message${error ? ' field__message--error' : ''}`} role={error ? 'alert' : undefined}>
          {error ?? helperText}
        </p>
      )}
    </div>
  )
}
