package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.OperatorDashboardSummary
import com.smartsolar.microgrid.domain.model.OperatorTransaction
import com.smartsolar.microgrid.domain.model.ReservationStatus

data class DashboardSummaryResponseDto(
    val pendingReservationCount: Long,
    val approvedFutureReservationCount: Long,
) {
    fun toDomain(): OperatorDashboardSummary = OperatorDashboardSummary(
        pendingReservationCount,
        approvedFutureReservationCount,
    )
}

data class VerifyTransactionRequestDto(
    val transactionReference: String,
)

data class OperatorTransactionResponseDto(
    val reservationId: String,
    val transactionReference: String,
    val status: String,
    val prosumerId: String,
    val stationId: String,
    val slotId: String,
    val scheduledTime: String,
) {
    fun toDomainOrNull(): OperatorTransaction? = ReservationStatus.fromBackendValue(status)?.let {
        OperatorTransaction(
            reservationId,
            transactionReference,
            it,
            prosumerId,
            stationId,
            slotId,
            scheduledTime,
        )
    }
}
