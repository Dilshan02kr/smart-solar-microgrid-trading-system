package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import java.time.Instant
import java.util.Locale

class BookingPresentationTest {
    @Test
    fun `date and time values are formatted without timezone conversion`() {
        assertEquals("Jan 2, 2026", BookingPresentation.date("2026-01-02T00:00:00Z", Locale.US))
        val displayedTime = BookingPresentation.time("09:30:00", Locale.US)
            .replace('\u202f', ' ')
            .replace('\u00a0', ' ')
        assertEquals("9:30 AM", displayedTime)
    }

    @Test
    fun `pending reservation at twelve hour cutoff is mutable`() {
        val now = Instant.parse("2026-01-01T00:00:00Z")
        assertTrue(BookingPresentation.canModify(reservation("2026-01-01T12:00:00Z", ReservationStatus.PENDING), now))
    }

    @Test
    fun `terminal and too-late reservations are not mutable`() {
        val now = Instant.parse("2026-01-01T00:00:00Z")
        assertFalse(BookingPresentation.canModify(reservation("2026-01-01T11:59:59Z", ReservationStatus.PENDING), now))
        assertFalse(BookingPresentation.canModify(reservation("2026-01-02T00:00:00Z", ReservationStatus.COMPLETED), now))
        assertFalse(BookingPresentation.canModify(reservation("2026-01-02T00:00:00Z", ReservationStatus.CANCELLED), now))
    }

    private fun reservation(time: String, status: ReservationStatus) = Reservation(
        reservationId = "reservation",
        prosumerId = "prosumer",
        stationId = "station",
        slotId = "slot",
        scheduledTime = time,
        status = status,
        transactionReference = null,
        createdAt = time,
        updatedAt = time,
        completedAt = null,
    )
}
