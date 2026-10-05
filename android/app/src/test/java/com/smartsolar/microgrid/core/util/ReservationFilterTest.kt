package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class ReservationFilterTest {
    private val pending = reservation("pending", ReservationStatus.PENDING)
    private val approved = reservation("approved", ReservationStatus.APPROVED, "TX-ABC-123")
    private val completed = reservation("completed", ReservationStatus.COMPLETED)
    private val cancelled = reservation("cancelled", ReservationStatus.CANCELLED)
    private val all = listOf(pending, approved, completed, cancelled)
    private val stations = mapOf("station" to Station("station", "Harbour Solar", "Colombo Port", 6.9, 79.8, 10.0, "Daily", StationStatus.ACTIVE, "", ""))

    @Test fun `blank search and all statuses returns everything`() = assertEquals(all, ReservationFilter.apply(all, stations))
    @Test fun `station name search is case insensitive`() = assertEquals(all, ReservationFilter.apply(all, stations, ReservationFilterCriteria("harbour SOLAR")))
    @Test fun `station location search works`() = assertEquals(all, ReservationFilter.apply(all, stations, ReservationFilterCriteria("colombo port")))
    @Test fun `unknown search returns empty`() = assertTrue(ReservationFilter.apply(all, stations, ReservationFilterCriteria("unknown")).isEmpty())
    @Test fun `pending filter returns pending only`() = assertEquals(listOf(pending), filtered(ReservationStatus.PENDING))
    @Test fun `approved filter returns approved only`() = assertEquals(listOf(approved), filtered(ReservationStatus.APPROVED))
    @Test fun `completed filter returns completed only`() = assertEquals(listOf(completed), filtered(ReservationStatus.COMPLETED))
    @Test fun `cancelled filter returns cancelled only`() = assertEquals(listOf(cancelled), filtered(ReservationStatus.CANCELLED))
    @Test fun `transaction reference search works`() = assertEquals(listOf(approved), ReservationFilter.apply(all, stations, ReservationFilterCriteria("abc-123")))
    @Test fun `reset restores blank search and all statuses`() = assertEquals(ReservationFilterCriteria(), ReservationFilter.reset())

    private fun filtered(status: ReservationStatus) = ReservationFilter.apply(all, stations, ReservationFilterCriteria(status = status))

    private fun reservation(id: String, status: ReservationStatus, reference: String? = null) = Reservation(
        reservationId = id,
        prosumerId = "prosumer",
        stationId = "station",
        slotId = "slot",
        scheduledTime = "2026-10-05T08:00:00Z",
        status = status,
        transactionReference = reference,
        createdAt = "",
        updatedAt = "",
        completedAt = null,
    )
}
