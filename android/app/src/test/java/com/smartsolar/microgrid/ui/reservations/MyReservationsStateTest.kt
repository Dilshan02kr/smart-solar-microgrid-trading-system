package com.smartsolar.microgrid.ui.reservations

import com.smartsolar.microgrid.core.util.ReservationGroup
import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Test

class MyReservationsStateTest {
    @Test
    fun `refreshed status moves reservation from pending to approved presentation`() {
        val pending = reservation(ReservationStatus.PENDING)
        val pendingState = MyReservationsState(
            reservations = listOf(pending),
            selectedGroup = ReservationGroup.PENDING,
        )

        assertEquals(listOf("reservation"), pendingState.visibleItems.map { it.reservation.reservationId })

        val refreshedReservations = listOf(pending.copy(status = ReservationStatus.APPROVED, transactionReference = "server-reference"))
        val refreshedPendingState = pendingState.copy(reservations = refreshedReservations)
        val refreshedApprovedState = refreshedPendingState.copy(selectedGroup = ReservationGroup.APPROVED)

        assertEquals(emptyList<ReservationListItem>(), refreshedPendingState.visibleItems)
        assertEquals(listOf("reservation"), refreshedApprovedState.visibleItems.map { it.reservation.reservationId })
    }

    private fun reservation(status: ReservationStatus) = Reservation(
        reservationId = "reservation",
        prosumerId = "prosumer",
        stationId = "missing-station",
        slotId = "slot",
        scheduledTime = "2026-10-06T10:00:00Z",
        status = status,
        transactionReference = null,
        createdAt = "2026-10-05T10:00:00Z",
        updatedAt = "2026-10-05T10:00:00Z",
        completedAt = null,
    )
}
