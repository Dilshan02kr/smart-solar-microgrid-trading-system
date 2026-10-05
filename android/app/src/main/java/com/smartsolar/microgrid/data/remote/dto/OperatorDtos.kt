package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.OperatorDashboardSummary
import com.smartsolar.microgrid.domain.model.OperatorReservation
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

data class UpdateOperatorSlotAvailabilityRequestDto(
    val isAvailable: Boolean,
)

data class OperatorReservationResponseDto(
    val reservationId: String,
    val prosumerId: String,
    val slotId: String,
    val scheduledTime: String,
    val status: String,
) {
    fun toDomainOrNull(): OperatorReservation? = ReservationStatus.fromBackendValue(status)?.let {
        OperatorReservation(reservationId, prosumerId, slotId, scheduledTime, it)
    }
}

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
