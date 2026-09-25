import { useEffect, useId, useRef } from 'react'
import { Button, type ButtonVariant } from '@/components/ui/Button'

export interface ConfirmationDialogProps {
  open: boolean
  title: string
  description: string
  confirmLabel: string
  confirmVariant?: ButtonVariant
  errorMessage?: string
  loading?: boolean
  onCancel: () => void
  onConfirm: () => void
}

export function ConfirmationDialog({
  open,
  title,
  description,
  confirmLabel,
  confirmVariant = 'primary',
  errorMessage,
  loading = false,
  onCancel,
  onConfirm,
}: ConfirmationDialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const titleId = useId()
  const descriptionId = useId()

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return

    if (open && !dialog.open) {
      dialog.showModal()
      dialog.querySelector<HTMLButtonElement>('button')?.focus()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={dialogRef}
      className="confirmation-dialog"
      aria-labelledby={titleId}
      aria-describedby={descriptionId}
      onCancel={(event) => {
        event.preventDefault()
        if (!loading) onCancel()
      }}
    >
      <h2 id={titleId} className="confirmation-dialog__title">{title}</h2>
      <p id={descriptionId} className="confirmation-dialog__description">{description}</p>
      {errorMessage && <p className="confirmation-dialog__error" role="alert">{errorMessage}</p>}
      <div className="confirmation-dialog__actions">
        <Button variant="outline" disabled={loading} onClick={onCancel}>Cancel</Button>
        <Button variant={confirmVariant} loading={loading} onClick={onConfirm}>{confirmLabel}</Button>
      </div>
    </dialog>
  )
}
