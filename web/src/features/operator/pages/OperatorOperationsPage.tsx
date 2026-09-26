import { useState } from 'react'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { completeReservation, verifyTransaction } from '@/features/operator/api/operatorApi'
import { TransactionVerificationForm } from '@/features/operator/components/TransactionVerificationForm'
import { VerifiedTransactionCard } from '@/features/operator/components/VerifiedTransactionCard'
import type { OperatorTransaction } from '@/features/operator/types/operatorTypes'
import { getOperatorErrorMessage } from '@/features/operator/utils/operatorPresentation'
import { ReservationStatus } from '@/features/reservations/types/reservationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function OperatorOperationsPage() {
  const [transactionReference, setTransactionReference] = useState('')
  const [transaction, setTransaction] = useState<OperatorTransaction | null>(null)
  const [verificationError, setVerificationError] = useState<ApiClientError | null>(null)
  const [completionError, setCompletionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [isVerifying, setIsVerifying] = useState(false)
  const [isCompleting, setIsCompleting] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [completionUnavailable, setCompletionUnavailable] = useState(false)

  async function handleVerify() {
    const normalizedReference = transactionReference.trim()
    if (!normalizedReference || isVerifying) return
    setIsVerifying(true)
    setVerificationError(null)
    setCompletionError(null)
    setSuccessMessage('')
    setTransaction(null)
    setCompletionUnavailable(false)
    try {
      const verified = await verifyTransaction({ transactionReference: normalizedReference })
      setTransaction(verified)
      setTransactionReference(normalizedReference)
      setSuccessMessage('Transaction verified for your assigned microgrid node.')
    } catch (requestError) {
      setVerificationError(toApiClientError(requestError))
    } finally {
      setIsVerifying(false)
    }
  }

  async function handleComplete() {
    if (!transaction || isCompleting || completionUnavailable) return
    setIsCompleting(true)
    setCompletionError(null)
    setSuccessMessage('')
    try {
      const completed = await completeReservation(transaction.reservationId)
      setTransaction(completed)
      setCompletionUnavailable(true)
      setSuccessMessage('Energy transfer completed successfully.')
      setConfirmOpen(false)
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      setCompletionError(normalized)
      if (normalized.code === 'RESERVATION_ALREADY_COMPLETED') {
        setTransaction((current) => current ? { ...current, status: ReservationStatus.COMPLETED } : current)
        setCompletionUnavailable(true)
        setConfirmOpen(false)
      } else if (normalized.code === 'RESERVATION_CANCELLED' || normalized.code === 'RESERVATION_NOT_APPROVED' || normalized.code === 'RESERVATION_NOT_FOUND') {
        setCompletionUnavailable(true)
        setConfirmOpen(false)
      } else if (normalized.code === 'ACCESS_DENIED' || normalized.code === 'OPERATOR_STATION_NOT_ASSIGNED') {
        setTransaction(null)
        setCompletionUnavailable(true)
        setConfirmOpen(false)
      }
    } finally {
      setIsCompleting(false)
    }
  }

  function reset() {
    setTransactionReference('')
    setTransaction(null)
    setVerificationError(null)
    setCompletionError(null)
    setSuccessMessage('')
    setCompletionUnavailable(false)
    window.requestAnimationFrame(() => document.getElementById('transaction-reference')?.focus())
  }

  return (
    <>
      <PageHeader title="Operator Operations" description="Verify a transaction reference and complete its approved energy transfer." />
      <PageContent className="operator-operations">
        <TransactionVerificationForm
          transactionReference={transactionReference}
          isVerifying={isVerifying}
          errorMessage={verificationError ? getOperatorErrorMessage(verificationError) : undefined}
          onReferenceChange={setTransactionReference}
          onSubmit={() => void handleVerify()}
        />
        {successMessage && <Alert variant="success" title={transaction?.status === ReservationStatus.COMPLETED ? 'Energy transfer completed' : 'Transaction verified'}>{successMessage}</Alert>}
        {completionError && <Alert variant="error" title="Unable to complete energy transfer">{getOperatorErrorMessage(completionError)}</Alert>}
        {transaction && <VerifiedTransactionCard transaction={transaction} completionUnavailable={completionUnavailable} onComplete={() => { setCompletionError(null); setConfirmOpen(true) }} onReset={reset} />}
      </PageContent>
      <ConfirmationDialog
        open={confirmOpen}
        title="Complete energy transfer?"
        description="Complete this energy transfer? The reservation will be finalized as COMPLETED and cannot return to an earlier state."
        confirmLabel="Complete Energy Transfer"
        confirmVariant="primary"
        errorMessage={completionError ? getOperatorErrorMessage(completionError) : undefined}
        loading={isCompleting}
        onCancel={() => { setConfirmOpen(false); setCompletionError(null) }}
        onConfirm={() => void handleComplete()}
      />
    </>
  )
}
