package com.smartsolar.microgrid.domain.model

data class OperatorDashboardSummary(
    val pendingReservationCount: Long,
    val approvedFutureReservationCount: Long,
)

data class OperatorTransaction(
    val reservationId: String,
    val transactionReference: String,
    val status: ReservationStatus,
    val prosumerId: String,
    val stationId: String,
    val slotId: String,
    val scheduledTime: String,
)
