package com.smartsolar.microgrid.domain.model

data class OperatorDashboardSummary(
    val pendingReservationCount: Long,
    val approvedFutureReservationCount: Long,
)

data class OperatorReservation(
    val reservationId: String,
    val prosumerId: String,
    val slotId: String,
    val scheduledTime: String,
    val status: ReservationStatus,
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
