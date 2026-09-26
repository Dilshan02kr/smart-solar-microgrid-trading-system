import type { FormEvent } from 'react'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'

export interface TransactionVerificationFormProps {
  transactionReference: string
  isVerifying: boolean
  errorMessage?: string
  onReferenceChange: (value: string) => void
  onSubmit: () => void
}

export function TransactionVerificationForm({
  transactionReference,
  isVerifying,
  errorMessage,
  onReferenceChange,
  onSubmit,
}: TransactionVerificationFormProps) {
  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!isVerifying) onSubmit()
  }

  return (
    <Card title="Verify transaction" description="Enter the server-issued transaction reference to verify an approved reservation for your assigned microgrid node.">
      <form className="transaction-verification-form" onSubmit={submit}>
        {errorMessage && <Alert variant="error" title="Verification failed">{errorMessage}</Alert>}
        <Input
          id="transaction-reference"
          label="Transaction Reference"
          value={transactionReference}
          onChange={(event) => onReferenceChange(event.target.value)}
          autoComplete="off"
          disabled={isVerifying}
          required
        />
        <div className="transaction-verification-form__actions">
          <Button type="submit" loading={isVerifying} disabled={isVerifying || !transactionReference.trim()}>Verify Transaction</Button>
        </div>
      </form>
    </Card>
  )
}
