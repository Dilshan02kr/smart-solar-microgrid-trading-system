package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.OperatorReservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class OperatorReservationPresentationTest {
    private val pendingLate = reservation("pending-late", ReservationStatus.PENDING, "2026-10-06T10:00:00Z")
    private val pendingEarly = reservation("pending-early", ReservationStatus.PENDING, "2026-10-05T08:00:00Z")
    private val approved = reservation("approved", ReservationStatus.APPROVED, "2026-10-05T09:00:00Z")

    @Test fun `pending grouping is chronological`() = assertEquals(
        listOf(pendingEarly, pendingLate),
        OperatorReservationPresentation.visible(listOf(pendingLate, approved, pendingEarly), ReservationStatus.PENDING),
    )

    @Test fun `approved grouping contains approved only`() = assertEquals(
        listOf(approved),
        OperatorReservationPresentation.visible(listOf(pendingLate, approved), ReservationStatus.APPROVED),
    )

    @Test fun `empty status result is empty`() = assertTrue(
        OperatorReservationPresentation.visible(listOf(pendingLate), ReservationStatus.APPROVED).isEmpty(),
    )

    private fun reservation(id: String, status: ReservationStatus, scheduled: String) = OperatorReservation(id, "prosumer", "slot", scheduled, status)
}
