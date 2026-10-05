package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.OperatorReservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import java.time.Instant

object OperatorReservationPresentation {
    fun visible(
        reservations: List<OperatorReservation>,
        status: ReservationStatus,
    ): List<OperatorReservation> = reservations
        .filter { it.status == status }
        .sortedBy { runCatching { Instant.parse(it.scheduledTime) }.getOrNull() ?: Instant.MAX }
}
