package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

class ReservationPresentationTest {
    @Test
    fun `groups exact statuses while preserving server order`() {
        val reservations = listOf(
            reservation("newer-completed", ReservationStatus.COMPLETED),
            reservation("approved", ReservationStatus.APPROVED, "server-reference"),
            reservation("cancelled", ReservationStatus.CANCELLED),
            reservation("pending", ReservationStatus.PENDING),
        )

        assertEquals(listOf("pending"), ReservationPresentation.inGroup(reservations, ReservationGroup.PENDING).map { it.reservationId })
        assertEquals(listOf("approved"), ReservationPresentation.inGroup(reservations, ReservationGroup.APPROVED).map { it.reservationId })
        assertEquals(
            listOf("newer-completed", "cancelled"),
            ReservationPresentation.inGroup(reservations, ReservationGroup.HISTORY).map { it.reservationId },
        )
    }

    @Test
    fun `only approved reservation with nonblank reference is QR eligible`() {
        assertEquals("exact-server-reference", ReservationPresentation.qrPayload(reservation("approved", ReservationStatus.APPROVED, "exact-server-reference")))
        assertNull(ReservationPresentation.qrPayload(reservation("pending", ReservationStatus.PENDING, "reference")))
        assertNull(ReservationPresentation.qrPayload(reservation("completed", ReservationStatus.COMPLETED, "reference")))
        assertNull(ReservationPresentation.qrPayload(reservation("cancelled", ReservationStatus.CANCELLED, "reference")))
        assertNull(ReservationPresentation.qrPayload(reservation("blank", ReservationStatus.APPROVED, "  ")))
    }

    @Test
    fun `QR payload is the exact unmodified server reference`() {
        val reference = "A1b2C3-server-value"
        assertEquals(reference, ReservationPresentation.qrPayload(reservation("approved", ReservationStatus.APPROVED, reference)))
    }

    private fun reservation(id: String, status: ReservationStatus, reference: String? = null) = Reservation(
        reservationId = id,
        prosumerId = "prosumer",
        stationId = "station",
        slotId = "slot",
        scheduledTime = "2026-01-02T00:00:00Z",
        status = status,
        transactionReference = reference,
        createdAt = "2026-01-01T00:00:00Z",
        updatedAt = "2026-01-01T00:00:00Z",
        completedAt = null,
    )
}
