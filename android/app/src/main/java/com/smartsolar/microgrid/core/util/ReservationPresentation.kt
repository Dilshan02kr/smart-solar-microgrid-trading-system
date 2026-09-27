package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus

enum class ReservationGroup {
    PENDING,
    APPROVED,
    HISTORY,
}

object ReservationPresentation {
    fun groupFor(status: ReservationStatus): ReservationGroup = when (status) {
        ReservationStatus.PENDING -> ReservationGroup.PENDING
        ReservationStatus.APPROVED -> ReservationGroup.APPROVED
        ReservationStatus.COMPLETED, ReservationStatus.CANCELLED -> ReservationGroup.HISTORY
    }

    fun inGroup(reservations: List<Reservation>, group: ReservationGroup): List<Reservation> =
        reservations.filter { groupFor(it.status) == group }

    fun qrPayload(reservation: Reservation): String? = reservation.transactionReference
        ?.takeIf { reservation.status == ReservationStatus.APPROVED && it.isNotBlank() }
}
